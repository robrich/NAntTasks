using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using System.Xml;

namespace Yahoo.Yui.Compressor.NAnt {

	[TaskName( "yuicompressor" )]
	public class YuiCompressor : Task {

		public YuiCompressor() {
			this.LineBreakPosition = -1;
			this.EncodingType = Encoding.Default;
		}

		private Compressor.CssCompressionType _cssCompressionType = Compressor.CssCompressionType.StockYuiCompressor;
		private ActionType _mode = ActionType.Unknown;

		#region Properties
		[TaskAttribute( "mode", Required = true )]
		public virtual string Mode { get; set; }
		[TaskAttribute( "outputfile", Required = true )]
		public virtual string OutputFile { get; set; }
		[BuildElement( "fileset", Required = true )]
		public virtual FileSet Files { get; set; }
		[TaskAttribute( "deletefiles", Required = false )]
		public virtual bool DeleteFiles { get; set; }
		[TaskAttribute( "encodingtype", Required = false )]
		public virtual Encoding EncodingType { get; set; }

		// CSS properties
		[TaskAttribute( "csscompressiontype", Required = false )] // StockYuiCompressor, MichaelAshRegexEnhancements, or Hybrid
		public virtual string CssCompressionType { get; set; }

		// JavaScript properties
		[TaskAttribute( "obfuscatejavascript", Required = false )]
		public virtual bool ObfuscateJavaScript { get; set; }
		[TaskAttribute( "preserveallsemicolons", Required = false )]
		public virtual bool PreserveAllSemicolons { get; set; }
		[TaskAttribute( "disableoptimizations", Required = false )]
		public virtual bool DisableOptimizations { get; set; }
		[TaskAttribute( "linebreakposition", Required = false )]
		public virtual int LineBreakPosition { get; set; }
		[TaskAttribute( "isevalignored", Required = false )]
		public virtual bool IsEvalIgnored { get; set; }
		#endregion

		protected override void InitializeTask( XmlNode taskNode ) {

			#region Required Elements

			if ( string.IsNullOrEmpty( Mode ) ) {
				throw new BuildException( "No Mode defined.", Location );
			}

			try {
				this._mode = (ActionType)Enum.Parse( typeof( ActionType ), this.Mode, true );
			} catch ( Exception ex ) {
				throw new BuildException( "Invalid Mode defined: " + Mode, Location, ex );
			}
			if ( this._mode != ActionType.Css && this._mode != ActionType.JavaScript ) {
				throw new BuildException( "Invalid Mode defined: " + Mode, Location );
			}

			if ( string.IsNullOrEmpty( OutputFile ) ) {
				throw new BuildException( "The OutputFile is null.", Location );
			}

			FileInfo _outputFile = new FileInfo( OutputFile );
			if ( !_outputFile.Directory.Exists ) {
				try {
					_outputFile.Directory.Create();
				} catch ( Exception ex ) {
					throw new BuildException( "Error creating the output directory: " + _outputFile.Directory.FullName, Location, ex );
				}
			}

			if ( this.Files == null || this.Files.FileNames.Count == 0 ) {
				throw new BuildException( "The <fileset> element must be used to specify the files to minify.", Location );
			}

			#endregion

			#region Optional Elements

			if ( string.IsNullOrEmpty( CssCompressionType ) ) {
				CssCompressionType = "YUIStockCompression";
			}

			switch ( CssCompressionType.ToLowerInvariant() ) {
				case "michaelashsregexenhancements":
					_cssCompressionType = Compressor.CssCompressionType.MichaelAshRegexEnhancements;
					break;
				case "havemycakeandeatit":
				case "bestofbothworlds":
				case "hybrid":
					_cssCompressionType = Compressor.CssCompressionType.Hybrid;
					break;
				default:
					_cssCompressionType = Compressor.CssCompressionType.StockYuiCompressor;
					break;
			}

			#endregion

		}


		/// <summary>
		/// Compress and combine files
		/// </summary>
		private string CompressFiles() {

			string actionDescription = this._mode.ToString();
			IList<string> fileList = null;
			string originalContent;
			int totalOriginalContentLength = 0;
			string compressedContent = null;
			StringBuilder finalContent = new StringBuilder();

			const string yep = "Yes";
			const string nope = "No";


			Log( Level.Verbose, string.Format( "# Found one or more {0} files. Now parsing ...", actionDescription ) );

			// Get the list of files to compress.
			if ( this._mode == ActionType.JavaScript ) {
				// First, lets display what javascript specific arguments have been specified.
				Log( Level.Verbose, "    ** Obfuscate Javascript: " + ( this.ObfuscateJavaScript ? yep : nope ) );
				Log( Level.Verbose, "    ** Preserve semi colons: " + ( this.PreserveAllSemicolons ? yep : nope ) );
				Log( Level.Verbose, "    ** Disable optimizations: " + ( this.DisableOptimizations ? yep : nope ) );
				Log( Level.Verbose, "    ** Line break position: " + ( this.LineBreakPosition <= -1 ? "None" : LineBreakPosition.ToString() ) );
			}

			string[] files = new string[this.Files.FileNames.Count];
			this.Files.FileNames.CopyTo( files, 0 );
			fileList = new List<string>( files );

			if ( fileList != null ) {
				Log( Level.Verbose, string.Format(
										 "# {0} {1} file{2} requested.",
										 fileList.Count,
										 actionDescription,
										 fileList.Count.ToPluralString() ) );

				// Now compress each file.
				foreach ( string file in fileList ) {

					Log( Level.Verbose, "" );
					Log( Level.Verbose, "=> " + file );

					FileInfo srcFile = new FileInfo( file );

					if ( !srcFile.Exists ) {
						throw new BuildException( file + " does not exist" );
					}

					// Load up the file.
					originalContent = File.ReadAllText( file );
					totalOriginalContentLength += originalContent.Length;

					if ( string.IsNullOrEmpty( originalContent ) ) {
						throw new BuildException( string.Format(
																 "There is no data in the file [{0}]. Please check that this is the file you want to compress. Lolz :)",
																 file ) );
					}
					
					try {
						if ( this._mode == ActionType.Css ) {
							compressedContent = CssCompressor.Compress( originalContent,
																		0,
																		_cssCompressionType );
						} else if ( this._mode == ActionType.JavaScript ) {
							compressedContent = JavaScriptCompressor.Compress( originalContent,
																			   file,
																			   this.Verbose,
																			   this.ObfuscateJavaScript,
																			   this.PreserveAllSemicolons,
																			   this.DisableOptimizations,
																			   this.LineBreakPosition,
																			   this.EncodingType,
																			   Thread.CurrentThread.CurrentCulture,
																			   this.IsEvalIgnored );
						}
					} catch ( Exception ex ) {
						throw new BuildException( "Failed to compress " + file, ex );
					}

					if ( !string.IsNullOrEmpty( compressedContent ) ) {
						finalContent.Append( compressedContent );
					}

				}

				Log( Level.Verbose, string.Format(
										 "Finished compressing all {0} file{1}.",
										 fileList.Count,
										 fileList.Count.ToPluralString() 
									) );

				int finalContentLength = finalContent.Length;

				Log( Level.Info, string.Format(
									"File size before: {0} chars. after: {1}. Result: {2}% of original size.",
									totalOriginalContentLength,
									finalContentLength,
									( 100 -
									  ( totalOriginalContentLength - (float)finalContentLength ) /
									  totalOriginalContentLength * 100 ).ToString( "f" )
									) );

				if ( this._mode == ActionType.Css ) {
					Log( Level.Info, string.Format(
											 "Css Compression Type: {0}.",
											 _cssCompressionType == Compressor.CssCompressionType.StockYuiCompressor
												 ? "Stock YUI compression"
												 :
													 _cssCompressionType ==
													 Compressor.CssCompressionType.MichaelAshRegexEnhancements
														 ? "Micahel Ash's Regex Enhancement compression"
														 : "Hybrid compresssion (the best compression out of all compression types)" ) );
				}
			}

			return finalContent.ToString();
		}

		private void SaveCompressedText( string compressedText ) {

			try {
				File.WriteAllText( this.OutputFile, compressedText ?? string.Empty );
				Log( Level.Verbose, string.Format( "Compressed content saved to file [{0}].", this.OutputFile ) );
			} catch ( Exception exception ) {
				// Most likely cause of this exception would be that the user failed to provide the correct path/file
				// or the file is read only, unable to be written, etc.. 
				throw new BuildException( string.Format(
										   "Failed to save the compressed text into the output file [{0}]. Please check the path/file name and make sure the file isn't magically locked, read-only, etc..",
										   this.OutputFile ), exception );
			}

		}

		private void DeleteTheFiles() {

			if ( DeleteFiles ) {
				foreach ( string file in this.Files.FileNames ) {

					FileInfo srcFile = new FileInfo( file );

					if ( srcFile.Exists ) {
						// Try and remove this file, if the user requests to do this.
						try {
							File.Delete( file );
						} catch ( Exception ex ) {
							throw new BuildException( string.Format( "Failed to delete the path/file [{0}] because " + ex.Message, file ), ex );
						}
					}
				}
			}
		}

		protected override void ExecuteTask() {

			Log( Level.Verbose, "Starting " + this._mode + " compression..." );
			DateTime startTime = DateTime.Now;

			string compressedText = CompressFiles();

			SaveCompressedText( compressedText );

			if ( this.DeleteFiles ) {
				DeleteTheFiles();
			}

			Log( Level.Verbose, "Finished " + this._mode + " compression." );
			Log( Level.Info, string.Format(
								"Total time to compress {0} {1} file{2} to [{3}]: {4} seconds",
								this.Files.FileNames.Count, // Assumes all files worked
								this._mode,
								this.Files.FileNames.Count.ToPluralString(),
								this.OutputFile,
								( DateTime.Now - startTime ).TotalSeconds.ToString( "f" )
				) );

		}


	}

	public enum ActionType {
		Unknown,
		Css,
		JavaScript
	}
}
