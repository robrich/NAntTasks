namespace CssValidator {

	#region using
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;
	using NAnt.Core;

	#endregion

	internal class CssValidatorHelper {

		public static Results ValidateFile(string JavaExe, string CssValidatorPath, string FileToCheck, string Profile, int Warning, int WaitSeconds) {

			if ( string.IsNullOrEmpty( JavaExe ) ) {
				throw new FileNotFoundException( "Can't find java.exe" );
			}
			// if it doesn't exist, PATH better be able to find it

			if ( string.IsNullOrEmpty( CssValidatorPath ) || !File.Exists( CssValidatorPath ) ) {
				throw new FileNotFoundException( "Can't find css-validator.jar at " + CssValidatorPath );
			}

			if ( string.IsNullOrEmpty( FileToCheck ) || !File.Exists( FileToCheck ) ) {
				throw new FileNotFoundException( "Can't find file to check: " + FileToCheck );
			}

			string cmd = string.Format( "-jar \"{0}\" file:\"{1}\" -output soap12", CssValidatorPath, FileToCheck );
			if ( !string.IsNullOrEmpty( Profile ) ) {
				cmd += " -profile " + Profile;
			}
			if ( Warning != 2 ) {
				cmd += " -warning " + Warning;
			}

			int waitSeconds = WaitSeconds;
			if ( waitSeconds < 1 ) {
				waitSeconds = 10;
			}


			string stdout = RunCmd( JavaExe, cmd, waitSeconds, FileToCheck );
			Results results = ParseStdOut( stdout );
			results.Cmd = JavaExe + " " + cmd;

			return results;
		}

		#region RunCmd
		private static string RunCmd( string exe, string cmd, int waitSeconds, string fileToCheck ) {

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
				Warnings = warningList,
				StdOut = stdout
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

		public string StdOut { get; set; }
		public string Cmd { get; set; }
	}

	internal class Error {
		public int Line { get; set; }
		public string ErrorDesc { get; set; }
		public string Message { get; set; }
	}

}
