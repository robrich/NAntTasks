namespace NAntSlnToProjects {

	using System;
	using System.IO;
	using System.Collections.Generic;

	public static class SolutionHelper {

		public static List<string> ParseSolution( FileInfo Solution ) {

			if ( Solution == null ) {
				throw new ArgumentNullException( "Solution" );
			}
			if ( !Solution.Exists ) {
				throw new FileNotFoundException( Solution.FullName + " not found", Solution.FullName );
			}

			List<string> projectPaths = new List<string>();

			string[] text = File.ReadAllLines( Solution.FullName );

			foreach ( string line in text ) {

				if ( string.IsNullOrEmpty( line ) ) {
					continue;
				}

				if ( !line.StartsWith( "Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC" ) // C#
					&& !line.StartsWith( "Project(\"{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}\")" ) // C++
					&& !line.StartsWith( "Project(\"{F184B08F-C81C-45F6-A57F-5ABD9991F28F}\")" ) // VB
					) {
					continue;
				}

				// Project("{guid}") = "Name", "Path", "{guid}"

				string[] parts = line.Substring( 52 ).Replace( "\"", "" ).Split( new char[] { ',' } );
				if ( parts.Length < 2 ) {
					throw new ArgumentOutOfRangeException( "Can't find project name and path in sln file line: " + line );
				}

				string name = parts[0].Trim();
				string path = parts[1].Trim();
				projectPaths.Add( path );

			}

			return projectPaths;
		}

		public static List<string> FilterProjects( string BasePath, List<string> Projects, bool ExistsOnly, ProjectTypes ProjectType ) {

			if ( Projects == null || Projects.Count <= 0 ) {
				return Projects; // You want nothing, you got it.
			}

			List<string> results = new List<string>();
			foreach ( string projectEntry in Projects ) {
				string project = Path.Combine( BasePath, projectEntry );

				if ( ExistsOnly ) {
					try {
						if ( !File.Exists( project ) ) {
							continue; // It doesn't
						}
					} catch ( Exception ) {
						continue; // It doesn't exist 'cause the path is bogus
					}
				}
				
				if ( ProjectType != ProjectTypes.All ) {
					
					// If it doesn't exist, we can't check it's type
					try {
						if ( !File.Exists( project ) ) {
							continue; // It doesn't
						}
					} catch ( Exception ) {
						continue; // It doesn't exist 'cause the path is bogus
					}

					string text = File.ReadAllText( project );
					if ( string.IsNullOrEmpty( text ) ) {
						// An empty project file isn't anything
						continue;
					}

					bool app = ( text.IndexOf( "<OutputType>Exe</OutputType>", StringComparison.InvariantCultureIgnoreCase ) > -1 )
						|| ( text.IndexOf( "<ConfigurationType>Application</ConfigurationType>", StringComparison.InvariantCultureIgnoreCase ) > -1 );
					bool web = ( text.IndexOf( "<Content Include=\"Web.config\"", StringComparison.InvariantCultureIgnoreCase ) > -1 )
								|| ( text.IndexOf( "{349c5851-65df-11da-9384-00065b846f21}", StringComparison.InvariantCultureIgnoreCase ) > -1 ); // Web Application Project
					bool test = ( text.IndexOf( "<Reference Include=\"nunit.framework", StringComparison.InvariantCultureIgnoreCase ) > -1 );

					switch ( ProjectType ) {
						case ProjectTypes.All: break; // Everything is fine
							
						case ProjectTypes.Endpoint: if ( !app && !web && !test ) { continue; } break;

						case ProjectTypes.Web: if ( !web ) { continue; } break;

						case ProjectTypes.App: if ( !app ) { continue; } break;

						case ProjectTypes.Test: if ( !test ) { continue; } break;

						default: throw new ArgumentOutOfRangeException( "ProjectType" );
					}

				}

				// We've passed all the tests
				results.Add( project );

			}

			return results;
		}

	}

	public enum ProjectTypes {
		All,
		Endpoint,
		Web,
		App,
		Test
	}

}
