namespace NAnt.Restrict.Filters {

	#region using
	using System.Collections.Generic;
	using NAnt.Core.Attributes;
	using NAnt.Restrict.Helpers;

	#endregion

	/// <summary>
	/// Passes if all of the contained filters pass
	/// </summary>
	[ElementName( "and" )]
	public class AndFilter : FilterNestedBase {

		public AndFilter()
			: this( null ) {
		}

		// For testing
		public AndFilter( List<FilterBase> Filters )
			: base( Filters ) {
		}

		public override bool Filter( IFileInfo File ) {
			bool results = true;

			foreach ( FilterBase filter in this.Filters ) {
				if ( !filter.Filter( File ) ) {
					results = false; // One failed
					break;
				}
			}

			return results;
		}

	}

}
