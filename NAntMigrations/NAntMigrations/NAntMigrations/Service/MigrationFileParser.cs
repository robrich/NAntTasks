namespace NAnt.DbMigrations.Tasks.Service {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	public interface IMigrationFileParser {
		Tuple<string, string> GetDownScript( string MigrationContent );
	}

	public class MigrationFileParser : IMigrationFileParser {

		/// <summary>
		/// A &quot;Down Script&quot; undoes the migration, putting the database back to it's original state<br />
		/// The down portion of the script is in a file comment marked with &quot;DOWN&quot; on a line of it's own
		/// </summary>
		/// <returns>The down script and the rest</returns>
		public Tuple<string,string> GetDownScript( string MigrationContent ) {
			string downScript = null;
			string theRest = null;

			if ( string.IsNullOrEmpty( MigrationContent ) ) {
				return new Tuple<string, string>( downScript, theRest ); // You asked for nothing, you got it
			}

			Regex regex = new Regex( @"/\*(?<comment>((?!\*/).)*)\*/[\r\n]*", RegexOptions.Singleline );
			MatchCollection matches = regex.Matches( MigrationContent );
			theRest = regex.Replace( MigrationContent, "" ); // Technically this removes all comments not just the "DOWN" one

			// Find first match
			foreach ( Match match in matches ) {
				string comment = match.Groups["comment"].Value.Trim();

				List<string> lines = comment.Split( new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries ).ToList();
				if ( (
					from l in lines
					where ( l ?? "" ).Trim().ToLowerInvariant() == "down"
					select l
				).Any() ) {
					// We've got a valid down script

					downScript = string.Join( Environment.NewLine, (
						from l in lines
						where string.IsNullOrEmpty( l )
						|| (l ?? "").Trim().ToLowerInvariant() != "down"
						select l
					) );

					break; // We found it
				}
			}

			return new Tuple<string, string>( downScript, theRest );
		}

	}
}
