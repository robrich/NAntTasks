namespace SassCommandLine {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text.RegularExpressions;
	using SassAndCoffee.Ruby.Sass;

	public static class Program {

		public static int Main( string[] TheArgs ) {

			bool compress = false;
			bool quiet = false;
			List<string> filenames = new List<string>();

			if ( TheArgs == null || TheArgs.Length < 1 ) {
				Console.Error.WriteLine( "No argumentes passed" );
				if ( !quiet ) {
					Help();
				}
				return -2;
			}

			List<string> args = new List<string>( TheArgs );
			while ( args.Count > 0 ) {
				string arg = args[0];
				args.RemoveAt( 0 );
				if ( string.IsNullOrEmpty( arg ) ) {
					continue;
				}
				if ( arg.StartsWith( "-" ) ) {
					switch ( arg ) {
						case "-compress":
							compress = true;
							break;
						case "-quiet":
						case "-q":
							quiet = true;
							break;
						default:
							Console.Error.WriteLine( "Unknown parameter: " + arg );
							if ( !quiet ) {
								Help();
							}
							return -2;
					}
				} else {
					// The rest is the list of files
					filenames.Add( arg );
					if ( args.Count > 0 ) {
						filenames.AddRange( args );
					}
					break;
				}
			}

			if ( filenames.Count < 1 ) {
				Console.Error.WriteLine( "No files to sass compile" );
				if ( !quiet ) {
					Help();
				}
				return -1;
			}

			SassCompiler compiler = new SassCompiler();
			Regex extensionRegex = new Regex( ".s(a|c)ss$" );

			foreach ( var filename in filenames ) {
				string targetFilename = extensionRegex.IsMatch( filename ) ? extensionRegex.Replace( filename, ".css" ) : ( filename + ".css" );
				try {
					if ( !File.Exists( filename ) ) {
						Console.Error.WriteLine( "Can't sass compile because file doesn't exist: " + filename );
						return -1;
					}
					string output = compiler.Compile( filename, compress, dependentFileList: null );
					File.WriteAllText( targetFilename, output );
				} catch ( Exception ex ) {
					File.WriteAllText( targetFilename, string.Format( "Error sass compiling {0}: {1}", filename, ex.Message ) );
					Console.Error.WriteLine( "Error compiling {0}: {1}", filename, ex.Message );
					return -1;
				}
			}

			return 0;
		}

		private static void Help() {
			Console.WriteLine( "SassCompiler [-compress] [-q[iet]] filename [filename2 [filename3 ...]]" );
			Console.WriteLine( "   -compress: compress the files" );
			Console.WriteLine( "      -quiet: don't display this help" );
			Console.WriteLine( " filename(s): the files to sass compile" );
		}

	}
}