namespace NAntSlnToProjects {

	#region using
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Core.Types;

	#endregion

	[TaskName( "slntoprojects" )]
	public class SlnToProjects : FileSet {

		private FieldInfo scannerField = null;
		private FieldInfo hasScannedField = null;

		public SlnToProjects()
			: base() {
			this.Init();
		}

		public SlnToProjects( FileSet fs )
			: base( fs ) {
			this.Init();
		}

		private void Init() {
			hasScannedField = GetField( typeof( FileSet ), "_hasScanned", typeof( bool ) );
			scannerField = GetField( typeof( FileSet ), "_scanner", typeof( DirectoryScanner ) );
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

		[TaskAttribute( "sln", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public string SolutionName { get; set; }

		/// <summary>
		/// Only return "endpoint" projects? e.g. has Web.config or builds to .exe or contains reference to NUnit
		/// </summary>
		[TaskAttribute( "type", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string ProjectType { get; set; }
		// "all", "endpoint", "web", "app", database, "test"

		[TaskAttribute( "existsonly", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public bool ExistsOnly { get; set; }

		#region FileSet properties that don't work here

		[Obsolete( "<slntoprojects ... /> is not used in this way.", true )]
		[TaskAttribute( "basedir" )]
		public virtual DirectoryInfo basedirBlock { get; set; }

		[Obsolete( "<slntoprojects ... /> is not used in this way.", true )]
		[BuildElementArray( "exclude" )]
		public Exclude[] excludeBlock { get; set; }

		[Obsolete( "<slntoprojects ... /> is not used in this way.", true )]
		[BuildElementArray( "excludesfile" )]
		public ExcludesFile[] excludesFileBlock { get; set; }

		[Obsolete( "<slntoprojects ... /> is not used in this way.", true )]
		[BuildElementArray( "include" )]
		public Include[] includeBlock { get; set; }

		[Obsolete( "<slntoprojects ... /> is not used in this way.", true )]
		[BuildElementArray( "includesfile" )]
		public IncludesFile[] includesFileBlock { get; set; }

		#endregion

		public override void Scan() {

			ProjectTypes projectType = ProjectTypes.Endpoint;
			if ( !string.IsNullOrEmpty( this.ProjectType ) ) {
				// If it isn't a valid enum, throw
				projectType = (ProjectTypes)Enum.Parse( typeof(ProjectTypes), this.ProjectType, true );
			}

			FileInfo sln = new FileInfo( this.SolutionName );
			if ( !sln.Exists ) {
				throw new BuildException( this.SolutionName + " doesn't exist, can't get the contents of it" );
			}

			this.BaseDirectory = sln.Directory;

			List<string> projects = SolutionHelper.ParseSolution( sln );

			projects = SolutionHelper.FilterProjects( sln.DirectoryName, projects, this.ExistsOnly, projectType );

			if ( projects != null ) {
				DirectoryScanner scanner = this.scanner; // Avoid re-reflecting every time
				foreach ( string filename in projects ) {
					scanner.FileNames.Add( filename );
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
