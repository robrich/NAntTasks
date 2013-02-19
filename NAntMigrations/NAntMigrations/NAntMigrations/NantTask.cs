namespace NAnt.DbMigrations.Tasks {
	using System;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.DbMigrations.Tasks.DataAccess;
	using NAnt.DbMigrations.Tasks.Infrastructure;
	using NAnt.DbMigrations.Tasks.Library;
	using NAnt.DbMigrations.Tasks.Presentation;
	using NAnt.DbMigrations.Tasks.Repository;
	using NAnt.DbMigrations.Tasks.Service;
	using NAnt.DbMigrations.Tasks.Service.Models;

	[TaskName("db-migrations")]
	public class NantTask : Task {

		[TaskAttribute( "migrationFilesSourcePath", Required = true )]
		public string MigrationFilesSourcePath { get; set; }

		[TaskAttribute( "connectionStringSource", Required = false )]
		public string ConnectionStringSource { get; set; }
		[TaskAttribute( "server", Required = false )]
		public string Server { get; set; }
		[TaskAttribute( "database", Required = false )]
		public string Database { get; set; }
		[TaskAttribute( "username", Required = false )]
		public string Username { get; set; }
		[TaskAttribute( "password", Required = false )]
		public string Password { get; set; }
		[TaskAttribute( "trustedConnection", Required = false )]
		public bool TrustedConnection { get; set; }

		[TaskAttribute( "currentAppVersion", Required = true )]
		public string CurrentAppVersion { get; set; }

		[TaskAttribute( "migrationScriptFile", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string MigrationScriptFile { get; set; }

		protected override void ExecuteTask() {

			ILogger logger = new ActionLogger( this.Log );

			// Lame DI solution here to avoid multiple dll dependencies for this tool
			IHashCalculator hashCalculator = new HashCalculator();
			ISqlHelper sqlHelper = new SqlHelper();
			IConfigRepository configRepository = new ConfigRepository();
			IMigrationDbRepository migrationDbRepository = new MigrationDbRepository( sqlHelper );
			IMigrationFileRepository migrationFileRepository = new MigrationFileRepository();
			ISqlCmdHelper sqlCmdHelper = new SqlCmdHelper( sqlHelper );
			IMigrationOutputFileRepository migrationOutputFileRepository = new MigrationOutputFileRepository();
			IMigrationFileParser migrationFileParser = new MigrationFileParser();
			IMigrationFileService migrationFileService = new MigrationFileService( hashCalculator, migrationFileRepository, migrationFileParser );
			IConnectionStringService connectionStringService = new ConnectionStringService( configRepository );
			IPathService pathService = new PathService();
			App app = new App( logger, migrationDbRepository, migrationFileRepository, sqlCmdHelper, migrationOutputFileRepository, migrationFileService, connectionStringService, pathService );

			DatabaseInfo databaseInfo = new DatabaseInfo {
				ConnectionStringSource = this.ConnectionStringSource,
				Server = this.Server,
				Database = this.Database,
				Username = this.Username,
				Password = this.Password,
				TrustedConnection = this.TrustedConnection
			};

			ExitReason result = app.RunMigrations( this.Project.BaseDirectory, this.Verbose, this.MigrationFilesSourcePath, databaseInfo, this.CurrentAppVersion, this.MigrationScriptFile );

			if ( this.FailOnError && result != ExitReason.Success ) {
				throw new BuildException( "Error in db-migration: "+result.ToString() );
			}
		}

	}

	public class ActionLogger : ILogger {

		public Action<Level, string> ToTheLog { get; set; }

		public ActionLogger( Action<Level, string> ToTheLog ) {
			this.ToTheLog = ToTheLog;
		}

		public void Log( string Message, Exception ex = null ) {
			string msg = null;
			if ( !string.IsNullOrEmpty( Message ) ) {
				msg = Message;
			}
			if ( ex != null ) {
				msg = "Error: " + ex.Message; // FRAGILE: This is probably unsanitary
			}
			if ( !string.IsNullOrEmpty( msg ) ) {
				this.ToTheLog( Level.Error, msg );
			}
		}

		public void Log( Exception ex ) {
			this.Log( null, ex );
		}

		public void Info( string Message ) {
			if ( !string.IsNullOrEmpty( Message ) ) {
				this.ToTheLog( Level.Info, Message );
			}
		}

	}
}
