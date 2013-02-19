namespace NAnt.Grep.Tasks {
	using System.Linq;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Core.Types;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;

	public class TermElement : Element {
		[TaskAttribute( "ignoreTerm", Required = false )]
		public string IgnoreTerm { get; set; }
	}

	[TaskName( "grep-test" )]
	public class GrepTask : Task {

		[BuildElement( "fileset", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public FileSet FilesToCheck { get; set; }

		[TaskAttribute( "searchTerm", Required = true )]
		public string SearchTerm { get; set; }

		[TaskAttribute( "ignoreTerm", Required = false )]
		public string IgnoreTerm { get; set; }

		[BuildElementCollection( "ignoreTerms", "term" )]
		public TermElement[] IgnoreTerms { get; set; }

		[TaskAttribute( "failOnFound", Required = false )]
		public bool FailOnFound { get; set; }

		[TaskAttribute( "failOnMissing", Required = false )]
		public bool FailOnMissing { get; set; }

		protected override void ExecuteTask() {

			string searchTerm = this.SearchTerm;
			List<string> ignoreTerms = new List<string>();
			if ( !string.IsNullOrWhiteSpace( this.IgnoreTerm ) ) {
				ignoreTerms.Add( this.IgnoreTerm );
			}
			if ( this.IgnoreTerms != null ) {
				ignoreTerms.AddRange(
					from term in this.IgnoreTerms
					where !string.IsNullOrWhiteSpace( term.IgnoreTerm )
					select term.IgnoreTerm
				);
			}

			FileSet filesToCheck = this.FilesToCheck;
			if ( filesToCheck == null || filesToCheck.FileNames.Count == 0 ) {
				throw new BuildException( "Can't grep an empty set of files" );
			}

			GrepEngine grepEngine = new GrepEngine();

			Dictionary<FileInfo, List<string>> results = new Dictionary<FileInfo, List<string>>();
			foreach ( string fileName in filesToCheck.FileNames ) {
				FileInfo file = new FileInfo( fileName );
				grepEngine.FindInFile( file, searchTerm, ignoreTerms, results );
			}

			if ( this.Verbose && results.Count > 0 ) {
				StringBuilder sb = new StringBuilder();
				foreach ( KeyValuePair<FileInfo, List<string>> entry in results ) {
					if ( sb.Length > 0 ) {
						sb.AppendLine();
					}
					string filename = entry.Key.FullName.Replace( this.Project.BaseDirectory + "\\", "" );
					sb.AppendLine( filename + ": " );
					foreach ( string line in entry.Value ) {
						sb.AppendLine( "  " + this.TrimToLength( line, 74 ) );
					}
				}
				this.Log( Level.Info, sb.ToString() );
			}

			bool found = results.Count > 0;
			if ( found && this.FailOnFound ) {
				string mess = "Found \"" + searchTerm + "\" in specified files";
				if ( !this.Verbose ) {
					mess += ", verbose=true to list matches";
				}
				throw new BuildException( mess );
			}
			if ( !found && this.FailOnMissing ) {
				throw new BuildException( "Did not find \"" + searchTerm + "\" in specified files" );
			}

		}

		private string TrimToLength( string Source, int Length ) {
			string val = ( Source ?? "" ).Trim();
			if ( !string.IsNullOrEmpty( val ) ) {
				if ( val.Length > Length ) {
					val = val.Substring( 0, Length ) + "...";
				}
			}
			return val;
		}

	}
}
