namespace NAnt.Restrict.Filters {

	#region using
	using System;
	using System.IO;
	using System.Text.RegularExpressions;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Restrict.Helpers;

	#endregion

	/// <summary>
	/// Selects files by name.
	/// </summary>
	[ElementName( "name" )]
	public class NameFilter : FilterBase {

		public NameFilter() : base() {
			this.CaseSensitive = true;
			this.HandleDirSep = false;
		}

		/// <summary>
		/// A string to search for in the file path and name, exactly one of name and regex are required
		/// </summary>
		[TaskAttribute( "name", Required = false )]
		public string StringFilter { get; set; }

		/// <summary>
		/// A regular expression to match against the file path and name, exactly one of name and regex are required
		/// </summary>
		[TaskAttribute( "regex", Required = false )]
		public string RegexFilter { get; set; }

		/// <summary>
		/// Whether comparisons are case-sensitive, default is true
		/// </summary>
		[TaskAttribute( "casesensitive", Required = false )]
		public bool CaseSensitive { get; set; }

		/// <summary>
		/// If this is specified, the mapper will treat a \ character in the filename as a / for the purposes of matching
		/// </summary>
		[TaskAttribute( "handledirsep", Required = false )]
		public bool HandleDirSep { get; set; }

		public override FilterPriority Priority {
			get { return FilterPriority.File; }
		}

		public override bool Filter( IFileInfo File ) {
			
			string stringPattern = ( this.StringFilter ?? "" ).Trim();
			string regexPattern = ( this.RegexFilter ?? "" ).Trim();
			bool casesensitive = this.CaseSensitive;

			if ( string.IsNullOrEmpty( stringPattern ) == string.IsNullOrEmpty( regexPattern ) ) {
				throw new BuildException( "<restrict> <name .../> </restrict> requires exactly one name=\"\" or regex=\"\"." );
			}

			string filename = File.FullName;
			if ( string.IsNullOrEmpty( filename ) ) {
				return false; // A blank filename doesn't match any filename filter
			}

			if ( this.HandleDirSep ) {
				filename = filename.Replace( "\\", "/" );
			}

			bool match = false; // It hasn't matched yet

			if ( !string.IsNullOrEmpty( stringPattern ) ) {
				// Check for a name match

				if ( casesensitive ) {
					// Case sensitive
					match = filename.Contains( stringPattern );
				} else {
					// Case insensitive
					match = filename.Contains( stringPattern, StringComparison.InvariantCultureIgnoreCase );
				}

			} else if ( !string.IsNullOrEmpty( regexPattern ) ) {
				// Check for a regex match

				RegexOptions options = RegexOptions.Singleline | RegexOptions.CultureInvariant;
				if ( !casesensitive ) {
					options |= RegexOptions.IgnoreCase;
				}
				try {
					match = Regex.IsMatch( filename, regexPattern, options );
				} catch (ArgumentException ex) {
					throw new BuildException( string.Format( "Error in Regex \"{0}\" on file \"{1}\", Message: {2}", regexPattern, filename, ex.Message ), ex );
				}
			}

			return match;
		}

	}

}
