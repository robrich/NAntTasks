namespace CssValidator {

	#region using
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;
	using NAnt.Core;
	using NAnt.Core.Attributes;

	#endregion

	[TaskName( "cssvalidator" )]
	public class CssValidatorTask : Task {

		public CssValidatorTask() {
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

		[TaskAttribute( "file", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public string FileToCheck { get; set; }

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
			string fileToCheck = this.FileToCheck;
			if ( !File.Exists( fileToCheck ) ) {
				throw new BuildException( "Can't cssvalidator a file that doesn't exist: " + fileToCheck );
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

			string cmd = string.Format( "-jar \"{0}\" file:\"{1}\" -output soap12", jar, fileToCheck );
			if ( !string.IsNullOrEmpty( profile ) ) {
				cmd += " -profile " + profile;
			}
			if ( warning != 2 ) {
				cmd += " -warning " + warning;
			}

			int waitSeconds = this.WaitSeconds;
			if ( waitSeconds < 1 ) {
				waitSeconds = 10;
			}


			string stdout = RunCmd( exe, cmd, waitSeconds, fileToCheck );
			var results = ParseStdOut( stdout );

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

			if ( this.FailOnError ) {
				if ( !results.Success || results.ErrorCount > 0 ) {
					throw new BuildException( string.Format( "Errors in {0}: {1}", fileToCheck, results.ErrorCount ) );
				}
			}

			if ( Project.Properties.Contains( prop ) ) {
				Project.Properties[prop] = results.ErrorCount.ToString();
			} else {
				Project.Properties.Add( prop, results.ErrorCount.ToString() );
			}
		}

		#region RunCmd
		private string RunCmd( string exe, string cmd, int waitSeconds, string fileToCheck ) {

			Log( Level.Verbose, ( "css-verifier: " + exe + " " + cmd ) );

			Process p = new Process {
				StartInfo = new ProcessStartInfo( exe, cmd ) {
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				},
				EnableRaisingEvents = false
			};

			string stderr = null;
			string stdout = null;
			int exitCode = 0;
			try {
				p.Start();

				// Wait for process to finish, and kill if it's not finished in predefined time
				bool finished = p.WaitForExit( waitSeconds * 1000 );
				if ( !finished ) {
					p.Kill();
					throw new BuildException( string.Format( "css-validator didn't finish checking {0} in {1} seconds", fileToCheck, waitSeconds ) );
				} else {
					exitCode = p.ExitCode;
					stdout = p.StandardOutput.ReadToEnd() ?? "";
				}
				stderr = p.StandardError.ReadToEnd() ?? "";
			} catch ( Exception ex ) {
				throw new BuildException( string.Format( "css-validator errored checking {0}: {1}", fileToCheck, ex.Message ), ex );
			} finally {
				p.Close();
			}

			if ( !string.IsNullOrEmpty( stderr ) ) {
				throw new BuildException( string.Format( "css-validator stderr was not blank checking {0}: {1}", fileToCheck, stderr ) );
			}
			if ( exitCode != 0 ) {
				throw new BuildException( string.Format( "css-validator exit code was not 0 checking {0}: {1}", fileToCheck, exitCode ) );
			}
			if ( string.IsNullOrEmpty( stdout ) ) {
				throw new BuildException( string.Format( "css-validator stdout was blank checking {0}", fileToCheck ) );
			}

			Log( Level.Verbose, ( "css-verifier stdout: " + stdout ) );
			return stdout;
		}
		#endregion

		#region ParseStdOut
		private static Results ParseStdOut( string stdout ) {
			var lines = stdout.Split(
				new char[] {
					'\n'
				} );
			string xml = string.Join( "\n", lines, 1, lines.Length - 1 ).Trim();
			XDocument doc = XDocument.Parse( xml );

			var resp = (
				from n in doc.Descendants()
				where n.Name.LocalName == "cssvalidationresponse"
				select n
				).FirstOrDefault();

			bool worked = bool.Parse(
				(
					from n in doc.Descendants()
					where n.Name.LocalName == "validity"
					select n.Value
					).First() );

			int errorCount = int.Parse(
				(
					from n in doc.Descendants()
					where n.Name.LocalName == "errorcount"
					select n.Value
					).First() );

			int warningCount = int.Parse(
				(
					from n in doc.Descendants()
					where n.Name.LocalName == "warningcount"
					select n.Value
					).First() );

			List<Error> errorList = (
				from e in doc.Descendants()
				where e.Name.LocalName == "error"
				select new Error {
					Line = int.Parse(
						(
							from d in e.Descendants()
							where d.Name.LocalName == "line"
							select d.Value.Trim()
							).First() ),
					ErrorDesc = (
						from d in e.Descendants()
						where d.Name.LocalName == "errortype"
						select d.Value.Trim()
						).First() + " " + (
							from d in e.Descendants()
							where d.Name.LocalName == "errorsubtype"
							select d.Value.Trim()
							).FirstOrDefault(),
					Message = Regex.Replace(
						Regex.Replace(
							Regex.Replace(
								(
									from d in e.Descendants()
									where d.Name.LocalName == "message"
									select d.Value
									).First(), " [ ]+", " ", RegexOptions.Multiline ),
							"(?:^ +| +$)", "", RegexOptions.Multiline ),
						"\n[ ]*$", "", RegexOptions.Singleline )
						.Replace( "\n\n", "\n" ).Replace( "\n", ", " ).Trim()
				}
				).ToList();

			List<Error> warningList = (
				from e in doc.Descendants()
				where e.Name.LocalName == "warning"
				select new Error {
					Line = int.Parse(
						(
							from d in e.Descendants()
							where d.Name.LocalName == "line"
							select d.Value.Trim()
							).First() ),
					ErrorDesc = (
						from d in e.Descendants()
						where d.Name.LocalName == "errortype"
						select d.Value.Trim()
						).First() + " " + (
							from d in e.Descendants()
							where d.Name.LocalName == "errorsubtype"
							select d.Value.Trim()
							).FirstOrDefault(),
					Message = Regex.Replace(
						Regex.Replace(
							Regex.Replace(
								(
									from d in e.Descendants()
									where d.Name.LocalName == "message"
									select d.Value
									).First(), " [ ]+", " ", RegexOptions.Multiline ),
							"(?:^ +| +$)", "", RegexOptions.Multiline ),
						"\n[ ]*$", "", RegexOptions.Singleline )
						.Replace( "\n\n", "\n" ).Replace( "\n", ", " ).Trim()
				}
				).ToList();

			if ( errorCount < errorList.Count ) {
				errorCount = errorList.Count;
			}

			if ( warningCount < warningList.Count ) {
				warningCount = warningList.Count;
			}

			if ( !worked && errorCount < 1 ) {
				errorCount = -1;
			}

			return new Results {
				Success = worked,
				ErrorCount = errorCount,
				WarningCount = warningCount,
				Errors = errorList,
				Warnings = warningList
			};
		}
		#endregion

	}

	internal class Results {
		public bool Success { get; set; }
		public int ErrorCount { get; set; }
		public int WarningCount { get; set; }
		public List<Error> Errors { get; set; }
		public List<Error> Warnings { get; set; }
	}

	internal class Error {
		public int Line { get; set; }
		public string ErrorDesc { get; set; }
		public string Message { get; set; }
	}

}