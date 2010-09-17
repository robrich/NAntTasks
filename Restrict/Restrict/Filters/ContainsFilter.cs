namespace NAnt.Restrict.Filters {

	#region using
	using System;
	using System.Text.RegularExpressions;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Restrict.Helpers;

	#endregion

	/// <summary>
	/// Selects resources whose contents match a specified string or regex
	/// </summary>
	[ElementName( "contains" )]
	public class ContainsFilter : FilterBase {

		public ContainsFilter() : base() {
			this.CaseSensitive = true;
		}

		/// <summary>
		/// A string to search for in the file's contents, exactly one of string and regex are required
		/// </summary>
		[TaskAttribute( "string", Required = false )]
		public string StringFilter { get; set; }

		/// <summary>
		/// A regular expression to match against the file's contents, exactly one of string and regex are required
		/// </summary>
		[TaskAttribute( "regex", Required = false )]
		public string RegexFilter { get; set; }

		/// <summary>
		/// Whether comparisons are case-sensitive, default is true
		/// </summary>
		[TaskAttribute( "casesensitive", Required = false )]
		public bool CaseSensitive { get; set; }

		public override FilterPriority Priority {
			get { return FilterPriority.Content; }
		}

		public override bool Filter( IFileInfo IFileInfo ) {
			
			string stringPattern = ( this.StringFilter ?? "" ).Trim();
			string regexPattern = ( this.RegexFilter ?? "" ).Trim();
			bool casesensitive = this.CaseSensitive;

			if ( string.IsNullOrEmpty( stringPattern ) == string.IsNullOrEmpty( regexPattern ) ) {
				throw new BuildException( "<restrict> <contains .../> </restrict> requires exactly one string=\"\" or regex=\"\"." );
			}

			string content = IFileInfo.Contents;
			if ( string.IsNullOrEmpty( content ) ) {
				return false; // A blank file doesn't match any filter
			}

			bool match = false; // It hasn't matched yet

			if ( !string.IsNullOrEmpty( stringPattern ) ) {
				// Check for a name match

				if ( casesensitive ) {
					// Case sensitive
					match = content.Contains( stringPattern );
				} else {
					// Case insensitive
					match = content.Contains( stringPattern, StringComparison.InvariantCultureIgnoreCase );
				}

			} else if ( !string.IsNullOrEmpty( regexPattern ) ) {
				// Check for a regex match

				RegexOptions options = RegexOptions.Multiline | RegexOptions.CultureInvariant;
				if ( !casesensitive ) {
					options |= RegexOptions.IgnoreCase;
				}
				try {
					match = Regex.IsMatch( content, regexPattern, options );
				} catch ( ArgumentException ex ) {
					throw new BuildException( string.Format( "Error in Regex: {0}, Message: {1}", regexPattern, ex.Message ), ex );
				}
			}

			return match;
		}

	}

}
