namespace NAnt.Grep.Tasks {
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class GrepEngine {

		public void FindInFile( FileInfo file, string search, Dictionary<FileInfo, List<string>> results ) {
			if ( !file.Exists ) {
				return;
			}
			string content = File.ReadAllText( file.FullName ) ?? "";
			if ( content.Contains( search ) ) {
				var lines = content.Split( '\n' ).Select( ( l, index ) => new {Line = l, LineNo = index} ).ToList();
				List<string> matches = (
					from l in lines
					where !string.IsNullOrWhiteSpace( l.Line )
					&& l.Line.Contains( search )
					select string.Format( "Line {0}: {1}", l.LineNo, l.Line.Trim() )
				).ToList();
				if ( matches != null && matches.Count > 0 ) {
					results.Add( file, matches );
				}
			}
		}

	}
}
