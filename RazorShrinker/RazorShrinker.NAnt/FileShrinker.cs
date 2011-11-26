namespace RazorShrinker.NAntTask {
	using System;
	using System.IO;

	public class FileShrinker {
		private readonly IContentShrinker contentShrinker;

		public FileShrinker(IContentShrinker ContentShrinker) {
			if ( ContentShrinker == null ) {
				throw new ArgumentNullException( "ContentShrinker" );
			}
			contentShrinker = ContentShrinker;
		}

		public void ShrinkFile( string Filename ) {
			if ( !File.Exists( Filename ) ) {
				throw new FileNotFoundException( "Can't shrink file because it doesn't exist: " + Filename, Filename );
			}
			string content = File.ReadAllText( Filename );
			string results = contentShrinker.ShrinkContent( content );
			File.WriteAllText( Filename, results );
		}

	}
}