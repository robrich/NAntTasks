namespace NAnt.Restrict.Filters {

	#region using
	using System.Collections.Generic;
	using NAnt.Core.Attributes;
	using NAnt.Restrict.Helpers;

	#endregion

	/// <summary>
	/// Selects a resource if it is selected by no nested resource selectors
	/// </summary>
	[ElementName( "none" )]
	public class NoneFilter : OrFilter {
		
		public NoneFilter()
			: this( null ) {
		}

		// For testing
		public NoneFilter( List<FilterBase> Filters )
			: base( Filters ) {
		}

		public override bool Filter( IFileInfo File ) {
			return !base.Filter( File );
		}

	}

}
