namespace NAnt.DbMigrations.Tasks.Library {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	// Inspired very, very loosly by http://ananthonline.net/2010/07/02/parsing-command-line-arguments-with-c-linq/

	public static class CommandLineParser {

		public static string GetHelpText( params ArgParameter[] ArgParameters ) {
			return GetHelpText( new List<ArgParameter>( ArgParameters ) );
		}
		public static string GetHelpText( List<ArgParameter> ArgParameters ) {
			return string.Join( Environment.NewLine, (
				from arg in ArgParameters
				select "  --" + arg.Name + ( !string.IsNullOrEmpty( arg.Abbreviation ) ? ( " [-" + arg.Abbreviation + "]" ) : null )
			).ToList() );
		}

		/// <summary>
		/// Parse command-line arguments
		/// </summary>
		/// <param name="args">User's input parameters</param>
		/// <param name="ArgParameters">Parameter definitions to match</param>
		/// <returns>Number of matched command line arguments - if this doesn't match the input args length, there were unmatched parameters</returns>
		public static int ParseCommandLineArguments( string[] args, params ArgParameter[] ArgParameters ) {
			return ParseCommandLineArguments( args, new List<ArgParameter>( ArgParameters ) );
		}

		/// <summary>
		/// Parse command-line arguments
		/// </summary>
		/// <param name="args">User's input parameters</param>
		/// <param name="ArgParameters">Parameter definitions to match</param>
		/// <returns>Number of matched command line arguments - if this doesn't match the input args length, there were unmatched parameters</returns>
		public static int ParseCommandLineArguments( string[] args, List<ArgParameter> ArgParameters ) {
			if ( ArgParameters.Count == 0 || args == null || args.Length == 0 ) {
				return 0;
			}

			Regex parser = new Regex( @"^([\-/]+)?(?<name>\w+)(\=(?<value>.+))?$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase );

			Dictionary<ArgParameter, List<string>> opts = (
				from arg in args
				from sw in ArgParameters
				let match = parser.Match( arg )
				where match.Success
				&& (
					string.Equals( sw.Name, match.Groups["name"].Value, StringComparison.InvariantCultureIgnoreCase )
					|| string.Equals( sw.Abbreviation, match.Groups["name"].Value, StringComparison.InvariantCultureIgnoreCase )
				)
				let swmatch = new { sw, match.Groups["value"].Value }
				group swmatch by swmatch.sw.Name into g
				select new {
					Switch = g.First().sw,
					Values = (
						from v in g
						select v.Value
					).ToList()
				}
			).ToDictionary( g => g.Switch, g => g.Values );

			foreach ( KeyValuePair<ArgParameter, List<string>> entry in opts ) {
				entry.Key.Process( entry.Value );
			}

			int matchedArgs = (
				from o in opts
				select o.Value.Count
			).Sum();

			return matchedArgs;
		}
	}

	// Class that encapsulates switch data. Not meant for direct use.
	public class ArgParameter {
		public ArgParameter( string name, string Abbreviation, Action<List<string>> Process ) {
			this.Name = name;
			this.Abbreviation = Abbreviation;
			this.Process = Process;
		}
		public ArgParameter( string name, Action<List<string>> Process ) {
			this.Name = name;
			this.Process = Process;
		}
		public string Name { get; set; }
		public string Abbreviation { get; set; }
		public Action<List<string>> Process { get; set; }
	}
}
