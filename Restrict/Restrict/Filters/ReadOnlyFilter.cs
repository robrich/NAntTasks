namespace NAnt.Restrict.Filters {

	#region using
	using System.IO;
	using NAnt.Core.Attributes;
	using NAnt.Restrict.Helpers;

	#endregion

	/// <summary>
	/// Selects read-only files
	/// </summary>
	[ElementName( "readonly" )]
	public class ReadOnlyFilter : FilterBase {

		public override FilterPriority Priority {
			get { return FilterPriority.File; }
		}

		public override bool Filter( IFileInfo File ) {
			if ( !File.Exists ) {
				return false; // It can't be read-only if it doesn't exist
			}
			return File.IsReadOnly;
		}

	}

}
