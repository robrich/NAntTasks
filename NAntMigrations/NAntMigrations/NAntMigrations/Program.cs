namespace NAnt.DbMigrations.Tasks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using NAnt.DbMigrations.Tasks.DataAccess;
	using NAnt.DbMigrations.Tasks.Infrastructure;
	using NAnt.DbMigrations.Tasks.Library;
	using NAnt.DbMigrations.Tasks.Presentation;
	using NAnt.DbMigrations.Tasks.Repository;
	using NAnt.DbMigrations.Tasks.Service;
	using NAnt.DbMigrations.Tasks.Service.Models;

	public static class Program {
		public static int Main( string[] args ) {

			string basePath = new Uri( Assembly.GetExecutingAssembly().CodeBase ).LocalPath;
			string migrationFilesSourcePath = null;
			DatabaseInfo databaseInfo = new DatabaseInfo();
			string currentAppVersion = null;
			string migrationScriptFile = null;
			bool wait = false;
			bool verbose = false;
			bool showHelp = false;

			var arguments = new List<ArgParameter> {
				new ArgParameter( "migrationFilesSourcePath", v => migrationFilesSourcePath = v.FirstOrDefault() ),
				new ArgParameter( "connectionStringSource", v => databaseInfo.ConnectionStringSource = v.FirstOrDefault() ),
				new ArgParameter( "server", "S", v => databaseInfo.Server = v.FirstOrDefault() ),
				new ArgParameter( "database", "d", v => databaseInfo.Database = v.FirstOrDefault() ),
				new ArgParameter( "username", "U", v => databaseInfo.Username = v.FirstOrDefault() ),
				new ArgParameter( "password", "P", v => databaseInfo.Password = v.FirstOrDefault() ),
				new ArgParameter( "trustedConnection", "E", v => databaseInfo.TrustedConnection = true ),
				new ArgParameter( "currentAppVersion", v => currentAppVersion = v.FirstOrDefault() ),
				new ArgParameter( "migrationScriptFile", v => migrationScriptFile = v.FirstOrDefault() ),
				new ArgParameter( "wait", "w", v => wait = true ),
				new ArgParameter( "verbose", "v", v => verbose = true ),
				new ArgParameter( "help", "?", v => showHelp = true )
			};

			int matchedArgs = CommandLineParser.ParseCommandLineArguments( args, arguments );
			if ( matchedArgs != args.Length ) {
				showHelp = true;
			}

			if ( showHelp ) {
				string helpContent = CommandLineParser.GetHelpText( arguments );
				Console.WriteLine( "NAnt.DbMigrations.Tasks" );
				Console.Write( helpContent );
				return (int)ExitReason.InvalidArguments;
			}

			ILogger logger = new ConsoleLogger();

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

			ExitReason result = app.RunMigrations( basePath, verbose, migrationFilesSourcePath, databaseInfo, currentAppVersion, migrationScriptFile );

			if ( wait ) {
				Console.WriteLine( "Migration complete, push any key to exit" );
				Console.ReadKey();
			}
			return (int)result;
		}
	}

	public class ConsoleLogger : ILogger {

		public void Log( string Message, Exception ex = null ) {
			if ( !string.IsNullOrEmpty( Message ) ) {
				Console.WriteLine( Message );
			}
			if ( ex != null ) {
				Console.WriteLine(
					"Error: " + ex.Message
#if DEBUG
					/*+ Environment.NewLine + ex.StackTrace
					 This is probably not sanitary */
#endif
				);
			}
		}

		public void Log( Exception ex ) {
			this.Log( null, ex );
		}

		public void Info( string Message ) {
			this.Log( Message );
		}

	}

}
