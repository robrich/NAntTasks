namespace NAnt.DbMigrations.Tasks.Repository {
	using System.IO;

	public interface IMigrationOutputFileRepository {
		void WriteFile( string DestinationPath, string Content );
	}

	public class MigrationOutputFileRepository : IMigrationOutputFileRepository {

		public void WriteFile( string DestinationPath, string Content ) {
			string targetFilename = DestinationPath;

			// Is the destination a file or a directory?
			if ( Directory.Exists( targetFilename ) || targetFilename.EndsWith( Path.PathSeparator.ToString() ) ) {
				targetFilename = Path.Combine( DestinationPath, "migrations.sql" );
			}

			// Write it
			if ( !string.IsNullOrEmpty( Content ) ) {
				File.WriteAllText( targetFilename, Content );
			} else if ( File.Exists( targetFilename ) ) {
				File.Delete( targetFilename );
			}
		}

	}
}
