namespace NAnt.DbMigrations.Tasks.Presentation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Transactions;
	using NAnt.DbMigrations.Tasks.Entity;
	using NAnt.DbMigrations.Tasks.Infrastructure;
	using NAnt.DbMigrations.Tasks.Repository;
	using NAnt.DbMigrations.Tasks.Service;
	using NAnt.DbMigrations.Tasks.Service.Models;

	public class App {
		private readonly ILogger logger;
		private readonly IMigrationDbRepository migrationDbRepository;
		private readonly IMigrationFileRepository migrationFileRepository;
		private readonly ISqlCmdHelper sqlCmdHelper;
		private readonly IMigrationOutputFileRepository migrationOutputFileRepository;
		private readonly IMigrationFileService migrationFileService;
		private readonly IConnectionStringService connectionStringService;
		private readonly IPathService pathService;

		public App( ILogger Logger, 
			IMigrationDbRepository MigrationDbRepository, IMigrationFileRepository MigrationFileRepository, ISqlCmdHelper SqlCmdHelper, IMigrationOutputFileRepository MigrationOutputFileRepository,
			IMigrationFileService MigrationFileService, IConnectionStringService ConnectionStringService, IPathService PathService ) {
			if ( Logger == null ) {
				throw new ArgumentNullException( "Logger" );
			}
			if ( MigrationDbRepository == null ) {
				throw new ArgumentNullException( "MigrationDbRepository" );
			}
			if ( MigrationFileRepository == null ) {
				throw new ArgumentNullException( "MigrationFileRepository" );
			}
			if ( SqlCmdHelper == null ) {
				throw new ArgumentNullException( "SqlCmdHelper" );
			}
			if ( MigrationOutputFileRepository == null ) {
				throw new ArgumentNullException( "MigrationOutputFileRepository" );
			}
			if ( MigrationFileService == null ) {
				throw new ArgumentNullException( "MigrationFileService" );
			}
			if ( ConnectionStringService == null ) {
				throw new ArgumentNullException( "ConnectionStringService" );
			}
			if ( PathService == null ) {
				throw new ArgumentNullException( "PathService" );
			}
			this.logger = Logger;
			this.migrationDbRepository = MigrationDbRepository;
			this.migrationFileRepository = MigrationFileRepository;
			this.sqlCmdHelper = SqlCmdHelper;
			this.migrationOutputFileRepository = MigrationOutputFileRepository;
			this.migrationFileService = MigrationFileService;
			this.connectionStringService = ConnectionStringService;
			this.pathService = PathService;
		}

		// TODO: This method is doing too much
		/// <param name="BaseDirectory">Normalize other paths to this directory</param>
		/// <param name="MigrationFilesSourcePath">Directory to pull migration files from</param>
		/// <param name="DatabaseInfo">Database connection details</param>
		/// <param name="CurrentAppVersion">The current git hash</param>
		/// <param name="MigrationScriptFile">The file to write the consolidated migration file into</param>
		/// <returns>The reason for exiting: success or failure</returns>
		public ExitReason RunMigrations( string BaseDirectory, bool Verbose, string MigrationFilesSourcePath, DatabaseInfo DatabaseInfo, string CurrentAppVersion, string MigrationScriptFile ) {

			if ( DatabaseInfo == null ) {
				this.logger.Log( "No connection string details sent" );
				return ExitReason.NoConnectionString;
			}

			string migrationFilesSourcePath = this.pathService.NormalizePath( BaseDirectory, MigrationFilesSourcePath );
			DatabaseInfo.ConnectionStringSource = this.pathService.NormalizePath( BaseDirectory, DatabaseInfo.ConnectionStringSource );
			string migrationScriptFile = this.pathService.NormalizePath( BaseDirectory, MigrationScriptFile );

			// Invalid config source
			if ( !this.migrationFileRepository.PathExists( migrationFilesSourcePath ) ) {
				this.logger.Log( "Migration files source path is not found" );
				return ExitReason.InvalidMigrationFileSourcePath;
			}

			// Get the db connection string
			string connnectionString = this.connectionStringService.GetConnectionString( DatabaseInfo );
			if ( string.IsNullOrEmpty( connnectionString ) ) {
				this.logger.Log( "No valid connection string found in the target configs" );
				return ExitReason.NoConnectionString;
			}

			if ( Verbose ) {
				this.logger.Info( "migrationFilesSourcePath: " + migrationFilesSourcePath );
				this.logger.Info( "currentAppVersion: " + CurrentAppVersion );
				this.logger.Info( "migrationScriptFile: " + migrationScriptFile );
				if ( !string.IsNullOrEmpty( DatabaseInfo.ConnectionStringSource ) ) {
					this.logger.Info( "connectionStringSource: " + DatabaseInfo.ConnectionStringSource );
				}
				this.logger.Info( "connectionString: " + connnectionString );
			}

			// Are both source paths valid?
			string connectTest = this.migrationDbRepository.TestConnection( connnectionString );
			if ( !string.IsNullOrEmpty(connectTest) ) {
				this.logger.Log( "Unable to connect to target database: " + connectTest );
				return ExitReason.InvalidDatabase;
			}

			// Ensure migration table exists
			// FRAGILE: Must be outside TransactionScope to query it, thus if the migrations fail, the table creation won't get rolled back
			this.migrationDbRepository.EnsureMigrationsTableExists( connnectionString );

			// Get all migrations from both db and files
			List<MigrationHistory> dbMigrations = this.migrationDbRepository.GetAllMigrations( connnectionString ) ?? new List<MigrationHistory>();
			List<MigrationFile> fileMigrations = this.migrationFileService.GetAllMigrations( migrationFilesSourcePath ) ?? new List<MigrationFile>();

			// Decide which migrations to process
			List<MigrationHistory> toRemove = this.MigrationsToRemove( dbMigrations, fileMigrations );
			List<MigrationFile> toAdd = this.MigrationsToAdd( fileMigrations, dbMigrations );

			// The output file
			StringBuilder migrationScript = new StringBuilder();

			// Create transaction scope
			using ( TransactionScope transaction = new TransactionScope() ) {
				try {

					if ( toRemove != null && toRemove.Count > 0 ) {
						foreach ( MigrationHistory row in toRemove ) {
							try {
								migrationScript.AppendLine( "-- Remove Migration " + row.Filename );

								if ( !string.IsNullOrEmpty( row.DownScript ) ) {
									// Run Down Migration
									this.sqlCmdHelper.RunSqlCommand( connnectionString, row.DownScript );
									migrationScript.AppendLine( row.DownScript );
									migrationScript.AppendLine( "GO" );
								}

								string deleteContent = this.migrationDbRepository.RemoveMigration( connnectionString, row );
								migrationScript.AppendLine( deleteContent );
								migrationScript.AppendLine( "GO" );
								migrationScript.AppendLine();

								this.logger.Info( "Successfully removed " + row.Filename );
							} catch {
								this.logger.Log( "Error removing " + row.Filename );
								throw;
							}
						}
					}
					
					if ( toAdd != null && toAdd.Count > 0 ) {
						foreach ( MigrationFile file in toAdd ) {
							try {
								// Run Migration
								this.sqlCmdHelper.RunSqlCommand( connnectionString, file.FileContent );

								// Log that migration was run
								string addContent = this.migrationDbRepository.AddMigration( connnectionString, new MigrationHistory {
									Filename = file.Filename,
									FileHash = file.FileHash,
									ExecutionDate = DateTime.Now,
									Version = CurrentAppVersion,
									DownScript = file.DownScript,
								} );

								migrationScript.AppendLine( "-- Add Migration " + file.Filename );
								migrationScript.AppendLine( file.FileContent );
								migrationScript.AppendLine( "GO" );
								migrationScript.AppendLine( addContent );
								migrationScript.AppendLine( "GO" );
								migrationScript.AppendLine();

								this.logger.Info( "Successfully added " + file.Filename );
							} catch {
								this.logger.Log( "Error adding " + file.Filename );
								throw;
							}
						}
					}

					transaction.Complete(); // Success
				} catch ( Exception ex ) {
					transaction.Dispose(); // Fail
					this.logger.Log( ex );
					return ExitReason.DatabaseError; // Probably
				}
			}

			string migrationScriptContent = migrationScript.ToString();
			if ( !string.IsNullOrEmpty( migrationScriptFile ) ) {
				// Create migration output script file
				// FRAGILE: ASSUME: it'll run the same as running them individually
				this.migrationOutputFileRepository.WriteFile( migrationScriptFile, migrationScriptContent );
			}
			return ExitReason.Success;
		}

		// public to be testable, not part of interface
		// Decide which db migrations to remove
		public List<MigrationHistory> MigrationsToRemove( List<MigrationHistory> dbMigrations, List<MigrationFile> fileMigrations ) {
			if ( dbMigrations == null || fileMigrations == null ) {
				return dbMigrations; // all of them
			}
			return (
				from d in dbMigrations
				where !fileMigrations.Any( f => f.Filename == d.Filename && f.FileHash == d.FileHash )
				orderby d.ExecutionDate descending // Newest first so we remove via lifo
				select d
			).ToList();
		}

		// public to be testable, not part of interface
		// Decide which file migrations to add
		public List<MigrationFile> MigrationsToAdd( List<MigrationFile> fileMigrations, List<MigrationHistory> dbMigrations ) {
			if ( fileMigrations == null || dbMigrations == null ) {
				return fileMigrations; // all of them
			}
			return (
				from f in fileMigrations
				where !dbMigrations.Any( d => d.Filename == f.Filename && d.FileHash == f.FileHash )
				orderby f.Filename // Alphabetically is deterministic but we really have no good sort oder
				select f
			).ToList();
		}

	}

	public enum ExitReason {
		Success = 0,
		InvalidArguments = -1,
		NoConnectionString = 1,
		InvalidDatabase = 2,
		InvalidMigrationFileSourcePath = 3,
		DatabaseError = 4,
	}
}
