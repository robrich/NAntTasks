namespace NAnt.Grep.Tasks {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class GrepEngine {

		public void FindInFile( FileInfo file, string search, List<string> ignore , Dictionary<FileInfo, List<string>> results ) {
			if ( !file.Exists ) {
				return;
			}
			string content = File.ReadAllText( file.FullName ) ?? "";
			if ( content.IndexOf( search, StringComparison.InvariantCultureIgnoreCase ) > -1 ) {
				var lines = content.Split( '\n' ).Select( ( l, index ) => new {Line = l, LineNo = index + 1} ).ToList();
				List<string> matches = (
					from l in lines
					where !string.IsNullOrWhiteSpace( l.Line )
					&& l.Line.IndexOf( search, StringComparison.InvariantCultureIgnoreCase ) > -1
					&& !ignore.Any( i => l.Line.IndexOf( i, StringComparison.InvariantCultureIgnoreCase ) > -1 )
					select string.Format( "Line {0}: {1}", l.LineNo, l.Line.Trim() )
				).ToList();
				if ( matches != null && matches.Count > 0 ) {
					results.Add( file, matches );
				}
			}
		}

	}
}
