namespace RazorShrinker.NAntTask {
	using System;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Core.Types;

	[TaskName( "razor-shrinker" )]
	public class RazorShrinkerTask : Task {

		[BuildElement( "fileset", Required = true )]
		public FileSet Files { get; set; }

		protected override void ExecuteTask() {

			FileShrinker fs = new FileShrinker( new ContentShrinker() );

			foreach ( string filename in this.Files.FileNames ) {
				try {
					this.Log( Level.Info, "Shrinking " + filename );
					fs.ShrinkFile( filename );
				} catch ( Exception ex ) {
					throw new BuildException( "Error shrinking " + filename + ": " + ex.Message, ex );
				}
			}

		}

	}
}