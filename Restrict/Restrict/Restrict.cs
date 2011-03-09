namespace NAnt.Restrict {

	#region using
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.IO;
	using System.Reflection;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Core.Types;
	using NAnt.Restrict.Filters;
	using NAnt.Restrict.Helpers;

	#endregion

	/// <summary>
	/// Filter a fileset based on passed criteria<br />
	/// <seealso cref="http://ant.apache.org/manual/Types/resources.html#restrict"/>
	/// </summary>
	[TaskName( "restrict" )]
	public class Restrict : FileSet {

		private FieldInfo scannerField = null;
		private FieldInfo hasScannedField = null;
		private List<FilterBase> filters = new List<FilterBase>();

		public Restrict()
			: base() {
			this.Init();
		}

		public Restrict( FileSet fs )
			: base( fs ) {
			this.Init();
		}

		// Parameter is used for testing
		public Restrict( List<FilterBase> Filters, FileSet Files )
			: this() {
			if ( Filters != null ) {
				this.filters.AddRange( Filters );
			}
			this.Files = Files;
		}

		// For testing
		public int NestedFilterCount {
			get { return filters.Count; }
		}

		private void Init() {
			hasScannedField = GetField( typeof( FileSet ), typeof( bool ), "_hasScanned" );
			scannerField = GetField( typeof( FileSet ), typeof( DirectoryScanner ), "_scanner" );
			this.Verbose = false;
		}

		#region FileSet fields that we reflect into

		protected DirectoryScanner scanner {
			get { return (DirectoryScanner)scannerField.GetValue( this ); }
		}

		public bool hasScanned {
			get { return (bool)hasScannedField.GetValue( this ); }
			protected set { hasScannedField.SetValue( this, value ); }
		}

		#endregion

		[BuildElement( "fileset", Required = true )]
		public FileSet Files { get; set; }

		[TaskAttribute( "verbose", Required = false )]
		public bool Verbose { get; set; }

		#region Add*Filter for each filter type ('cause a generic FilterBase doesn't seem to work)

		[BuildElement( "name" )]
		public void AddNameFilter( NameFilter name ) {
			this.filters.Add( name );
		}

		[BuildElement( "exists" )]
		public void AddExistsFilter( ExistsFilter exists ) {
			this.filters.Add( exists );
		}

		[BuildElement( "date" )]
		public void AddDateFilter( DateFilter date ) {
			this.filters.Add( date );
		}

		[BuildElement( "filesize" )]
		public void AddFileSizeFilter( FileSizeFilter filesize ) {
			this.filters.Add( filesize );
		}

		[BuildElement( "and" )]
		public void AddAndFilter( AndFilter and ) {
			this.filters.Add( and );
		}

		[BuildElement( "or" )]
		public void AddOrFilter( OrFilter or ) {
			this.filters.Add( or );
		}

		[BuildElement( "not" )]
		public void AddNotFilter( NotFilter not ) {
			this.filters.Add( not );
		}

		[BuildElement( "none" )]
		public void AddNoneFilter( NoneFilter none ) {
			this.filters.Add( none );
		}

		[BuildElement( "majority" )]
		public void AddMajorityFilter( MajorityFilter majority ) {
			this.filters.Add( majority );
		}

		[BuildElement( "contains" )]
		public void AddContainsFilter( ContainsFilter contains ) {
			this.filters.Add( contains );
		}

		[BuildElement( "readonly" )]
		public void AddReadOnlyFilter( ReadOnlyFilter readonlyfilter ) {
			this.filters.Add( readonlyfilter );
		}

		#endregion

		#region FileSet properties that don't work here

		[Obsolete( "<restrict ... /> is not used in this way.", true )] // TODO: How to block NAn't use of it while allowing it for code?
		[TaskAttribute( "basedir" )]
		public override DirectoryInfo BaseDirectory {
			get { return this.Files.BaseDirectory; }
			set { /* ignore */ }
		}

		[Obsolete( "<restrict ... /> is not used in this way.", true )]
		[BuildElementArray( "exclude" )]
		public Exclude[] excludeBlock { get; set; }

		[Obsolete( "<restrict ... /> is not used in this way.", true )]
		[BuildElementArray( "excludesfile" )]
		public ExcludesFile[] excludesFileBlock { get; set; }

		[Obsolete( "<restrict ... /> is not used in this way.", true )]
		[BuildElementArray( "include" )]
		public Include[] includeBlock { get; set; }

		[Obsolete( "<restrict ... /> is not used in this way.", true )]
		[BuildElementArray( "includesfile" )]
		public IncludesFile[] includesFileBlock { get; set; }

		#endregion

		#region Scan
		public override void Scan() {

			if ( this.Files == null ) {
				throw new BuildException( "<fileset .../> is not defined." );
			}
			if ( this.filters.Count == 0 ) {
				throw new BuildException( "<restrict .../> has no filters defined." );
			}
			this.filters.Sort( ( a, b ) => ( (int)a.Priority ).CompareTo( (int)b.Priority ) );

			foreach ( FilterBase filter in this.filters ) {
				FilterNestedBase nested = filter as FilterNestedBase;
				if ( nested != null ) {
					nested.NestedInitialize();
				}
			}

			DirectoryScanner scanner = this.scanner; // Avoid re-reflecting every time
			try {
				StringEnumerator fileEnumerator = this.Files.FileNames.GetEnumerator();
				while ( fileEnumerator.MoveNext() ) {
					string filename = fileEnumerator.Current;
					if ( string.IsNullOrEmpty( filename ) ) {
						throw new ArgumentNullException( "filename" );
					}
					IFileInfo fi = new IFileInfo( filename );

					// Filter it
					bool passed = true;
					foreach ( FilterBase filter in this.filters ) {
						if ( !filter.Filter( fi ) ) {
							// This filter said not to use it
							passed = false;
							if ( this.Verbose ) {
								this.Log( Level.Info, "failed by " + filter.Name + ": " + filename );
							}
							continue;
						}
					}

					if ( passed ) {
						scanner.FileNames.Add( filename );
						if ( this.Verbose ) {
							this.Log( Level.Info, "passed: " + filename );
						}
					}
				}

				this.hasScanned = true;
			} catch ( Exception ex ) {
				throw new BuildException( "Error building restricted results", this.Location, ex );
			}
			if ( this.FailOnEmpty && ( this.scanner.FileNames.Count == 0 ) ) {
				throw new ValidationException( "No matching files when filtering in " + this.BaseDirectory, this.Location );
			}
		}
		#endregion

		#region GetField (Reflection)
		private static FieldInfo GetField( Type TargetType, Type FieldType, string FieldName ) {
			FieldInfo field = TargetType.GetField( FieldName, BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy );
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
