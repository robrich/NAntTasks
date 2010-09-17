namespace NAnt.Restrict.Filters {

	#region using
	using System.Collections.Generic;
	using NAnt.Core.Attributes;
	using NAnt.Restrict.Helpers;

	#endregion

	/// <summary>
	/// Passes if one or more of the contained filters pass
	/// </summary>
	[ElementName( "or" )]
	public class OrFilter : FilterNestedBase {
		
		public OrFilter()
			: this( null ) {
		}

		// For testing
		public OrFilter( List<FilterBase> Filters )
			: base( Filters ) {
		}

		public override bool Filter( IFileInfo File ) {
			bool results = false;

			foreach ( FilterBase filter in this.Filters ) {
				if ( filter.Filter( File ) ) {
					results = true; // One passed
					break;
				}
			}

			return results;
		}

	}

}
