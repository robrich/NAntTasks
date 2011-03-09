namespace NantProjectContent {

	#region using
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Core.Types;

	#endregion

	[TaskName( "projecttocontent" )]
	public class ProjectToContent : FileSet {

		private FieldInfo scannerField = null;
		private FieldInfo hasScannedField = null;

		public ProjectToContent()
			: base() {
			this.Init();
		}

		public ProjectToContent( FileSet fs )
			: base( fs ) {
			this.Init();
		}

		private void Init() {
			hasScannedField = GetField( typeof( FileSet ), "_hasScanned", typeof( bool ) );
			scannerField = GetField( typeof( FileSet ), "_scanner", typeof( DirectoryScanner ) );
			this.Verbose = false;
		}

		#region private fields in FileSet that we reflect into

		protected DirectoryScanner scanner {
			get { return (DirectoryScanner)scannerField.GetValue( this ); }
			set { scannerField.SetValue( this, value ); }
		}

		protected bool hasScanned {
			get { return (bool)hasScannedField.GetValue( this ); }
			set { hasScannedField.SetValue( this, value ); }
		}

		#endregion

		#region FileSet properties that don't work here

		[Obsolete( "<projecttocontent ... /> is not used in this way.", true )] // TODO: How to block NAn't use of it while allowing it for code?
		[TaskAttribute( "basedir" )]
		public override DirectoryInfo BaseDirectory { get; set; }

		[Obsolete( "<projecttocontent ... /> is not used in this way.", true )]
		[BuildElementArray( "exclude" )]
		public Exclude[] excludeBlock { get; set; }

		[Obsolete( "<projecttocontent ... /> is not used in this way.", true )]
		[BuildElementArray( "excludesfile" )]
		public ExcludesFile[] excludesFileBlock { get; set; }

		[Obsolete( "<projecttocontent ... /> is not used in this way.", true )]
		[BuildElementArray( "include" )]
		public Include[] includeBlock { get; set; }

		[Obsolete( "<projecttocontent ... /> is not used in this way.", true )]
		[BuildElementArray( "includesfile" )]
		public IncludesFile[] includesFileBlock { get; set; }

		#endregion

		[TaskAttribute( "proj", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public string ProjectName { get; set; }

		[TaskAttribute( "verbose", Required = false )]
		public bool Verbose { get; set; }

		public override void Scan() {

			FileInfo project = new FileInfo( this.ProjectName );
			if ( this.Verbose ) {
				this.Log( Level.Info, "Scanning for content in " + this.ProjectName );
			}
			if ( !project.Exists ) {
				throw new BuildException( this.ProjectName + " doesn't exist, can't get the contents of it" );
			}

			this.BaseDirectory = project.Directory;

			List<string> content = ProjectHelper.GetProjectContentViaXml( project );

			DirectoryScanner scanner = this.scanner; // Avoid re-reflecting every time
			foreach ( string filename in content ) {
				scanner.FileNames.Add( filename );
				if ( this.Verbose ) {
					this.Log( Level.Info, filename );
				}
			}
			this.hasScanned = true;

			if ( this.FailOnEmpty && ( this.scanner.FileNames.Count == 0 ) ) {
				throw new ValidationException( "No matching files when filtering in " + this.BaseDirectory, this.Location );
			}
		}

		#region GetField (Reflection)
		private static FieldInfo GetField( Type BaseType, string FieldName, Type FieldType ) {
			var field = BaseType.GetField( FieldName, BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy );
			if ( field == null ) {
				throw new ArgumentNullException( FieldName, "Can't find private field " + FieldName );
			}
			if ( field.FieldType != FieldType ) {
				throw new ArgumentNullException( FieldName, "Found the field " + FieldName + ", but it isn't a " + FieldType.FullName + ", it's a " + field.FieldType.FullName );
			}
			return field;
		}
		#endregion

	}

}
