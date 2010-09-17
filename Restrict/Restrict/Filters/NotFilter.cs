namespace NAnt.Restrict.Filters {

	#region using
	using System.Collections.Generic;
	using NAnt.Core.Attributes;
	using NAnt.Restrict.Helpers;

	#endregion

	/// <summary>
	/// Negates the result of the nested filters
	/// </summary>
	[ElementName( "not" )]
	public class NotFilter : AndFilter {
		
		public NotFilter()
			: this( null ) {
		}

		// For testing
		public NotFilter( List<FilterBase> Filters )
			: base( Filters ) {
		}

		public override bool Filter( IFileInfo File ) {
			return !base.Filter( File );
		}

	}

}
