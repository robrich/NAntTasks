namespace NAnt.Restrict.Helpers {

	#region using
	using System;

	#endregion

	// http://groups.google.com/group/microsoft.public.dotnet.languages.csharp/browse_thread/thread/ebc4258ed2c4270f/c48b6c15df01e8bf

	public static class StringCompareCaseInsensitive {

		public static bool Contains( this string strSearch, string strFind, StringComparison sc ) {

			if ( string.IsNullOrEmpty( strSearch ) || string.IsNullOrEmpty( strFind ) ) {
				return false;
			}

			for ( int ich = 0; ich < strSearch.Length - strFind.Length; ich++ ) {
				if ( strSearch.Substring( ich, strFind.Length ).Equals( strFind, sc ) ) {
					return true;
				}
			}

			return false;
		}

	}

}
