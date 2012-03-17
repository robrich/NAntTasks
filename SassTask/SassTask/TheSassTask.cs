namespace NAnt.SassTask {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Core.Types;
	using SassAndCoffee.Ruby.Sass;
	using System.Text.RegularExpressions;

	[TaskName( "sass" )]
	public class TheSassTask : Task {

		public TheSassTask() {
			this.Compress = false;
		}

		[TaskAttribute( "compress" )]
		public bool Compress { get; set; }

		[BuildElement( "fileset", Required = true )]
		public FileSet Files { get; set; }

		protected override void ExecuteTask() {

			SassCompiler compiler = new SassCompiler();
			Regex extensionRegex = new Regex( ".s(a|c)ss$" );

			foreach ( string filename in this.Files.FileNames ) {
				string targetFilename = extensionRegex.IsMatch( filename ) ? extensionRegex.Replace( filename, ".css" ) : ( filename + ".css" );
				try {
					this.Log( Level.Info, "sass compiling " + filename );
					if ( !File.Exists( filename ) ) {
						throw new Exception( "Can't sass compile because file doesn't exist: " + filename );
					}
					string output = compiler.Compile( filename, this.Compress, dependentFileList: null );
					File.WriteAllText( targetFilename, output );
				} catch ( Exception ex ) {
					File.WriteAllText( targetFilename, string.Format( "Error sass compiling {0}: {1}", filename, ex.Message ) );
					throw new BuildException( "Error sass compiling " + filename + ": " + ex.Message, ex );
				}
			}
			this.Log( Level.Info, "sass compiling all files complete" );

		}

	}
}