namespace NAnt.DbMigrations.Tasks.Service {
	using System.IO;

	public interface IPathService {
		string NormalizePath( string BaseDirectory, string UserPath );
	}

	public class PathService : IPathService {

		public string NormalizePath( string BaseDirectory, string UserPath ) {
			string results = null;
			string userPath = ( UserPath ?? "" ).Replace( '/', '\\' );
			if ( string.IsNullOrEmpty( userPath ) || string.IsNullOrEmpty( BaseDirectory ) ) {
				results = userPath;
			} else if ( Path.IsPathRooted( userPath ) ) {
				results = userPath;
			} else {
				results = Path.GetFullPath( Path.Combine( BaseDirectory, userPath ) );
			}
			return results;
		}

	}
}
