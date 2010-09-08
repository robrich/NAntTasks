namespace CssValidator {

	#region using
	using System;
	using System.IO;
	using System.Reflection;
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


			foreach ( string fileToCheck in filesToCheck.FileNames ) {
				if ( !File.Exists( fileToCheck ) ) {
					throw new BuildException( fileToCheck + " doesn't exist" );
				}
				Results results = CssValidatorHelper.ValidateFile( exe, jar, fileToCheck, profile, warning, waitSeconds );

				// TODO: If passed "output as xml", build this output differently
				if ( this.Verbose ) {
					if ( !string.IsNullOrEmpty( results.Cmd ) ) {
						Log( Level.Verbose, results.Cmd );
					}
					if ( !string.IsNullOrEmpty( results.StdOut ) ) {
						Log( Level.Verbose, results.StdOut );
					}
				}
				if ( results.Errors != null && results.Errors.Count > 0 ) {
					foreach ( Error error in results.Errors ) {
						Log( Level.Error, string.Format( "Error: {0} ({1}): {2}, {3}", fileToCheck, error.Line, error.ErrorDesc, error.Message ) );
					}
				}
				if ( results.Warnings != null && results.Warnings.Count > 0 ) {
					foreach ( Error error in results.Warnings ) {
						Log( Level.Warning, string.Format( "Warning: {0} ({1}): {2}, {3}", fileToCheck, error.Line, error.ErrorDesc, error.Message ) );
					}
				}

				errorCount += results.ErrorCount;
				if ( !results.Success && results.ErrorCount < 1 ) {
					errorCount++; // To insure the build fails correctly
				}

			}

			if ( this.FailOnError ) {
				if ( errorCount > 0 ) {
					throw new BuildException( string.Format( "Errors found in css files: ", errorCount ) );
				}
			}

			if ( Project.Properties.Contains( prop ) ) {
				Project.Properties[prop] = errorCount.ToString();
			} else {
				Project.Properties.Add( prop, errorCount.ToString() );
			}

		}
	}
	
}