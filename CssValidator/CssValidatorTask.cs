namespace CssValidator {

	#region using
	using System;
	using System.IO;
	using System.Reflection;
	using System.Xml.Linq;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Core.Types;

	#endregion

	[TaskName( "cssvalidator" )]
	public class CssValidatorTask : Task {

		public CssValidatorTask() {
			this.FilesToCheck = new FileSet();
			this.Warning = 2;
			this.Profile = "css21";
			this.JavaExe = "java.exe";
			this.WaitSeconds = 10;
			this.BasePath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().GetName().CodeBase.Substring( 8 ) ); // Get past file://
		}

		#region Properties

		public string BasePath { get; set; }

		[TaskAttribute( "javaexe", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string JavaExe { get; set; }

		/// <summary>
		/// How many seconds to wait until we give up and kill the thread
		/// </summary>
		[TaskAttribute( "waitseconds", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public int WaitSeconds { get; set; }

		[BuildElement( "fileset", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public FileSet FilesToCheck { get; set; }

		/// <summary>
		/// Profile passed to css-validator
		/// </summary>
		[TaskAttribute( "profile", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string Profile { get; set; }

		/// <summary>
		/// Medium passed to css-validator
		/// </summary>
		[TaskAttribute( "medium", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string Medium { get; set; }

		/// <summary>
		/// Warning level passed to css-validator
		/// </summary>
		[TaskAttribute( "warning", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public int Warning { get; set; }

		[TaskAttribute( "errorcountproperty", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string ErrorCountProperty { get; set; }

		[TaskAttribute( "xmloutputfile", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string XmlOutputFile { get; set; }

		#endregion

		protected override void ExecuteTask() {
			FileSet filesToCheck = this.FilesToCheck;
			if ( filesToCheck == null || filesToCheck.FileNames.Count == 0 ) {
				throw new BuildException( "Can't cssvalidator an empty set of files" );
			}

			int warning = this.Warning;
			string profile = this.Profile;
			if ( string.IsNullOrEmpty( profile ) ) {
				profile = "css21";
			}
			string prop = this.ErrorCountProperty;

			string exe = this.JavaExe;
			if ( string.IsNullOrEmpty( exe ) || !File.Exists( exe ) ) {
				exe = "java.exe"; // If java isn't in your path, this will blow a rubber ball
			}

			string jar = Path.Combine( BasePath, "css-validator.jar" );
			if ( !File.Exists( jar ) ) {
				throw new BuildException( "Can't find css-validator.jar at " + BasePath );
			}

			int waitSeconds = this.WaitSeconds;
			if ( waitSeconds < 1 ) {
				waitSeconds = 10;
			}

			int errorCount = 0;
			int warningCount = 0;

			XDocument doc = null;
			XElement root = null;
			if ( !string.IsNullOrEmpty( this.XmlOutputFile ) ) {
				doc = new XDocument();
				root = new XElement( "css-validator" );
				doc.AddFirst( root );
			}

			foreach ( string fileToCheck in filesToCheck.FileNames ) {
				if ( !File.Exists( fileToCheck ) ) {
					throw new BuildException( fileToCheck + " doesn't exist" );
				}
				Results results = CssValidatorHelper.ValidateFile( exe, jar, fileToCheck, profile, warning, waitSeconds );

				if ( this.Verbose ) {
					if ( !string.IsNullOrEmpty( results.Cmd ) ) {
						Log( Level.Verbose, results.Cmd );
					}
					if ( !string.IsNullOrEmpty( results.StdOut ) ) {
						// This is just too verbose: Log( Level.Debug, results.StdOut );
					}
				}
				if ( this.Verbose || string.IsNullOrEmpty( this.XmlOutputFile ) ) {
					if ( results.Errors != null && results.Errors.Count > 0 ) {
						foreach ( Error error in results.Errors ) {
							Log( Level.Error, string.Format( "{0}: {1} ({2}): {3}, {4}", error.ErrorType, fileToCheck, error.Line, error.ErrorDesc, error.Message ) );
						}
					}
				}
				if ( doc != null ) {
					XElement element = new XElement(
						"file",
						new XAttribute( "name", fileToCheck ),
						new XAttribute( "errors", results.ErrorCount ),
						new XAttribute( "warnings", results.WarningCount )
						);
					root.Add( element );
					if ( results.Errors != null && results.Errors.Count > 0 ) {
						foreach ( Error error in results.Errors ) {
							element.Add(
								new XElement(
									"error",
									new XAttribute( "line", error.Line ),
									new XAttribute( "type", error.ErrorType ),
									new XElement( "description", error.ErrorDesc ),
									new XElement( "message", error.Message )
								) );
						}
					}
					if ( this.Verbose ) {
						element.Add( new XElement( "command", results.Cmd ) );
						element.Add( new XElement( "stdout", results.StdOut ) );
					}
				}

				errorCount += results.ErrorCount;
				warningCount += results.WarningCount;
				if ( !results.Success && results.ErrorCount < 1 ) {
					errorCount++; // To insure the build fails correctly
				}

			}

			if ( doc != null ) {
				root.Add( new XAttribute( "success", ( errorCount == 0 ) ) );
				root.Add( new XAttribute( "totalErrors", errorCount ) );
				root.Add( new XAttribute( "totalWarnings", warningCount ) );
				doc.Save( this.XmlOutputFile );
			}

			if ( this.FailOnError && errorCount > 0 ) {
				throw new BuildException( "Errors found in css files: " + errorCount );
			}

			if ( Project.Properties.Contains( prop ) ) {
				Project.Properties[prop] = errorCount.ToString();
			} else {
				Project.Properties.Add( prop, errorCount.ToString() );
			}

		}
	}
	
}