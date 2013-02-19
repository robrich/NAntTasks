namespace NAnt.DbMigrations.Tasks.Repository {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public interface IMigrationFileRepository {
		bool PathExists( string MigrationPath );

		/// <summary>
		/// Doesn't include path
		/// </summary>
		List<string> GetFilenames( string MigrationPath );

		string GetFileContent( string MigrationFilePath, string Filename );
	}

	public class MigrationFileRepository : IMigrationFileRepository {

		public bool PathExists( string MigrationPath ) {
			bool exists = false;
			try {
				exists = Directory.Exists( MigrationPath );
			} catch ( Exception ) {
				// Doesn't matter why it didn't work, it didn't
				exists = false;
			}
			return exists;
		}

		/// <summary>
		/// Doesn't include path
		/// </summary>
		public List<string> GetFilenames( string MigrationPath ) {
			DirectoryInfo dir = new DirectoryInfo( MigrationPath );
			return (
				from f in dir.GetFiles( "*.sql" )
				orderby f.Name
				select f.Name
			).ToList();
		}

		public string GetFileContent( string MigrationFilePath, string Filename ) {
			string path = Path.Combine( MigrationFilePath, Filename );
			string content = null;
			if ( File.Exists( path ) ) {
				content = File.ReadAllText( path );
			}
			return content;
		}

	}
}
