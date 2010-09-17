namespace NAnt.Restrict.Filters {

	#region using
	using System.Collections.Generic;
	using NAnt.Core;
	using NAnt.Core.Attributes;

	#endregion

	public abstract class FilterNestedBase : FilterBase {
		
		// Parameter is used for testing
		public FilterNestedBase( List<FilterBase> Filters ) {
			this.Filters = new List<FilterBase>();
			if ( Filters != null ) {
				this.Filters.AddRange( Filters );
			}
		}

		// For testing
		public int NestedFilterCount {
			get { return Filters.Count; }
		}

		protected List<FilterBase> Filters { get; set; }

		#region Add*Filter for each filter type ('cause a generic FilterBase doesn't seem to work)

		[BuildElement( "name" )]
		public void AddNameFilter( NameFilter name ) {
			this.Filters.Add( name );
		}

		[BuildElement( "exists" )]
		public void AddExistsFilter( ExistsFilter exists ) {
			this.Filters.Add( exists );
		}

		[BuildElement( "date" )]
		public void AddDateFilter( DateFilter date ) {
			this.Filters.Add( date );
		}

		[BuildElement( "filesize" )]
		public void AddFileSizeFilter( FileSizeFilter filesize ) {
			this.Filters.Add( filesize );
		}

		[BuildElement( "and" )]
		public void AddAndFilter( AndFilter and ) {
			this.Filters.Add( and );
		}

		[BuildElement( "or" )]
		public void AddOrFilter( OrFilter or ) {
			this.Filters.Add( or );
		}

		[BuildElement( "not" )]
		public void AddNotFilter( NotFilter not ) {
			this.Filters.Add( not );
		}

		[BuildElement( "none" )]
		public void AddNoneFilter( NoneFilter none ) {
			this.Filters.Add( none );
		}

		[BuildElement( "majority" )]
		public void AddMajorityFilter( MajorityFilter majority ) {
			this.Filters.Add( majority );
		}

		[BuildElement( "contains" )]
		public void AddContainsFilter( ContainsFilter contains ) {
			this.Filters.Add( contains );
		}

		[BuildElement( "readonly" )]
		public void AddReadOnlyFilter( ReadOnlyFilter readonlyfilter ) {
			this.Filters.Add( readonlyfilter );
		}

		#endregion

		public override FilterPriority Priority {
			get { return FilterPriority.Nested; }
		}

		/// <summary>
		/// Validate and sort the filters
		/// </summary>
		public virtual void NestedInitialize() {

			if ( this.Filters == null || this.Filters.Count == 0 ) {
				throw new BuildException( "<restrict .../> filter has no nested filters defined." );
			}
			this.Filters.Sort( ( a, b ) => ( (int)a.Priority ).CompareTo( (int)b.Priority ) );

			foreach ( FilterBase filter in this.Filters ) {
				FilterNestedBase nested = filter as FilterNestedBase;
				if ( nested != null ) {
					nested.NestedInitialize(); // Recurse into children
				}
			}

		}

	}

}
