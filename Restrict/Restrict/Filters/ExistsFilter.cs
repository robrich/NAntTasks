namespace NAnt.Restrict.Filters {

	#region using
	using System.IO;
	using NAnt.Core.Attributes;
	using NAnt.Restrict.Helpers;

	#endregion

	/// <summary>
	/// Selects files that exist in the file system
	/// </summary>
	[ElementName( "exists" )]
	public class ExistsFilter : FilterBase {
		
		public override FilterPriority Priority {
			get { return FilterPriority.File; }
		}

		public override string Description() {
			return "<exists />";
		}

		public override bool Filter( IFileInfo File ) {
			return File.Exists;
		}

	}

}
