namespace NAnt.DbMigrations.Tasks.Repository {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;

	public interface IConfigRepository {
		XElement GetXmlContent( FileInfo File );
		List<FileInfo> GetConfigFiles( string BasePath );
	}

	public class ConfigRepository : IConfigRepository {

		public XElement GetXmlContent( FileInfo File ) {
			try {
				return XElement.Load( File.FullName );
			} catch ( Exception ) {
				return null; // Bad file
			}
		}

		public List<FileInfo> GetConfigFiles( string BasePath ) {
			List<FileInfo> results = null;
			// Is it a file or a directory?
			if ( File.Exists( BasePath ) ) {
				// It's a file
				results = new List<FileInfo> {
					new FileInfo( BasePath )
				};
			} else if ( Directory.Exists( BasePath ) ) {
				// It's a directory
				DirectoryInfo dir = new DirectoryInfo( BasePath );
				results = (
					from d in this.GetFolders( dir ) ?? new List<DirectoryInfo>()
					let files = d.GetFiles( "*.config" )
					from f in files
					select f
					).ToList();
			} else {
				// It doesn't exist
				results = null;
			}
			return results;
		}

		private List<DirectoryInfo> GetFolders( DirectoryInfo Directory ) {
			var results = new List<DirectoryInfo>();
			if ( Directory.Exists ) {
				results.Add( Directory );
				DirectoryInfo[] children = Directory.GetDirectories();
				if ( children != null && children.Length > 0 ) {
					foreach ( DirectoryInfo child in children ) {
						List<DirectoryInfo> step = this.GetFolders( child );
						if ( step != null && step.Count > 0 ) {
							results.AddRange( step );
						}
					}
				}
			}
			return results;
		}

	}
}
