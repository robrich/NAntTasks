namespace NantProjectContent {

	#region using
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;
	//using Microsoft.Build.BuildEngine;

	#endregion

	public static class ProjectHelper {
		
		/*
		// This method is much more robust, and works fine in a console app, but blows chunks in an NAnt task because VSVersion is null
		public static List<string> GetProjectContent( string ProjectFile ) {

			ValidateProjectPath( ProjectFile );

			List<string> results = new List<string>();

			//Engine.GlobalEngine.BinPath = @"C:\Windows\Microsoft.NET\Framework\v2.0.50727";
			Project project = new Project();
			project.Load( ProjectFile );
			foreach ( BuildItemGroup itemGroup in project.ItemGroups ) {
				foreach ( BuildItem item in itemGroup ) {
					if ( item.Name == "Content" ) {
						results.Add( item.Include );
					}
				}
			}

			return results;
		}
		*/

		public static List<string> GetProjectContentViaXml( FileInfo ProjectFile ) {

			if ( ProjectFile == null ) {
				throw new ArgumentNullException( "ProjectFile" );
			}
			if ( !ProjectFile.Exists ) {
				throw new FileNotFoundException( ProjectFile.FullName + " not found", ProjectFile.FullName );
			}

			List<string> results = new List<string>();

			string path = ProjectFile.Directory.FullName;
			
			XDocument doc = XDocument.Load( ProjectFile.FullName );
			var items = (
				from c in doc.Descendants()
				where c.HasAttributes
				&& c.Name == "{http://schemas.microsoft.com/developer/msbuild/2003}Content"
				&& c.Attribute( "Include" ) != null
				select c.Attribute( "Include" ).Value as string
				).ToList();

			results = ResolveItems( path, items );

			return results;
		}

		//
		// The brains behind resolving wildcard references
		//

		private static List<string> ResolveItems( string BasePath, List<string> items ) {
			if ( items == null || items.Count == 0 ) {
				return items; // Nothing harvests nothing
			}

			List<string> results = new List<string>();

			foreach ( var item in items ) {
				if ( item.Contains( "*" ) ) {
					// Resolve what this means in the file system

					if ( item.Contains( "**" ) ) {
						// Resolve directory wild-card

						string before = BasePath;
						string after = "*.*";
						int pos = item.IndexOf( "**" );
						if ( pos > 0 ) {
							before = Path.Combine( BasePath, item.Substring( 0, pos ) );
							if ( before.EndsWith( "\\" ) ) {
								before = before.Substring( 0, before.Length - 1 );
							}
						}
						if ( pos < ( item.Length - 2 ) ) {
							after = item.Substring( pos + 2 );
							if ( after.StartsWith( "\\" ) ) {
								after = after.Substring( 1 );
							}
						}

						List<string> resultsStep1 = ResolveWildcardDirectory( before, null, after );
						if ( resultsStep1 != null && resultsStep1.Count > 0 ) {
							results.AddRange( resultsStep1 );
						} else {
							string mess = "No files found at " + Path.Combine( BasePath, item );
							throw new ArgumentNullException( mess );
						}

					} else {
						// Resolve the files in this directory

						List<string> resultsStep2 = ResolveWildcard( BasePath, BasePath, item );
						if ( resultsStep2 != null && resultsStep2.Count > 0 ) {
							results.AddRange( resultsStep2 );
						} else {
							string mess = "No files found at " + Path.Combine( BasePath, item );
							throw new ArgumentNullException( mess );
						}

					}
				} else {
					results.Add( item );
				}
			}

			return results;
		}

		private static List<string> ResolveWildcard( string BasePath, string CurrentPath, string FilePattern ) {

			List<string> results = new List<string>();

			DirectoryInfo d = new DirectoryInfo( CurrentPath );
			FileInfo[] files = null;
			try {
				files = d.GetFiles( FilePattern );
			} catch ( DirectoryNotFoundException ) {
				// Ignore the fact that we just trapsed into a directory that doesn't exist
				files = new FileInfo[0];
			}
			if ( files == null || files.Length == 0 ) {
				return results; // Nothing found
			}
			Console.WriteLine( BasePath );
			foreach ( FileInfo f in files ) {
				string fullPath = f.FullName;
				// Trim off the BasePath if we can
				int pos = fullPath.IndexOf( BasePath );
				if ( pos > -1 ) {
					fullPath = fullPath.Substring( pos + BasePath.Length + 1 ); // After \
				}
				results.Add( fullPath );
			}

			return results;
		}

		private static List<string> ResolveWildcardDirectory( string BasePath, DirectoryInfo CurrentDirectory, string FilePattern ) {

			List<string> results = new List<string>();

			DirectoryInfo d = CurrentDirectory ?? new DirectoryInfo( BasePath );

			if ( CurrentDirectory != null ) {
				List<string> resultsStep = ResolveWildcard( BasePath, d.FullName, FilePattern );
				if ( resultsStep != null && resultsStep.Count > 0 ) {
					results.AddRange( resultsStep );
				}
			} else {
				// Resolving **\something means it's definitely not in the current directory
			}

			DirectoryInfo[] childDirectories = d.GetDirectories();
			if ( childDirectories != null && childDirectories.Length > 0 ) {
				// Recurse
				foreach ( DirectoryInfo dir in childDirectories ) {
					List<string> resultsStep = ResolveWildcardDirectory( BasePath, dir, FilePattern );
					if ( resultsStep != null && resultsStep.Count > 0 ) {
						results.AddRange( resultsStep );
					}
				}
			}

			return results;
		}

	}

}
