namespace NAnt.Restrict.Filters {

	#region using
	using System.Collections.Generic;
	using NAnt.Core.Attributes;
	using NAnt.Restrict.Helpers;

	#endregion

	/// <summary>
	/// Selects a resource if it is selected by the majority of nested resource selectors
	/// </summary>
	[ElementName( "majority" )]
	public class MajorityFilter : FilterNestedBase {

		public MajorityFilter()
			: this( null, AllowTie: true ) {
		}

		// For testing
		public MajorityFilter( List<FilterBase> Filters, bool AllowTie )
			: base( Filters ) {
			this.AllowTie = AllowTie;
		}

		/// <summary>
		/// Whether a tie (when there is an even number of nested resource selectors) is considered a majority, default true
		/// </summary>
		[TaskAttribute( "allowtie", Required = false )]
		public bool AllowTie { get; set; }

		public override bool Filter( IFileInfo File ) {
			int failCount = 0;
			int passCount = 0;

			foreach ( FilterBase filter in this.Filters ) {
				if ( filter.Filter( File ) ) {
					passCount++;
				} else {
					failCount++;
				}
			}

			bool results = false;

			if ( passCount == failCount ) {
				results = AllowTie;
			} else {
				results = ( passCount > failCount );
			}

			return results;
		}

	}

}
