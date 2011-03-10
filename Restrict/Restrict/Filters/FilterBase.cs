namespace NAnt.Restrict.Filters {

	#region using
	using System.Text;
	using NAnt.Core;
	using NAnt.Restrict.Helpers;

	#endregion

	public abstract class FilterBase : Element {

		/// <summary>
		/// The priority this should run so easier to run stuff (filename) can run before harder to run stuff (file contents)
		/// </summary>
		public abstract FilterPriority Priority { get; }

		/// <summary>
		/// A pretty description to show that describes what this filter does including parameters
		/// </summary>
		/// <returns></returns>
		public abstract string Description();

		/// <summary>
		/// Does this file match this filter?
		/// </summary>
		/// <param name="File">The file</param>
		/// <returns>Does it match the filter? yes or no</returns>
		public abstract bool Filter( IFileInfo File );

		protected void AddParameter( StringBuilder sb, string ParameterName, object ParameterValue ) {
			sb.Append( ParameterName );
			sb.Append( "=\"" );
			sb.Append( ParameterValue );
			sb.Append( "\" " );
		}

	}

}
