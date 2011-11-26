namespace RazorShrinker.NAntTask {
	using System.Text.RegularExpressions;

	public interface IContentShrinker {
		string ShrinkContent( string Content );
	}

	public class ContentShrinker : IContentShrinker {

		// FRAGILE:
		// If you have a comment in a string, it will fail
		// e.g. @{ string c = "@*something*@"; }  yields  @{ string c = ""; }  but should yield  @{ string c = "@*something*@"; }

		public string ShrinkContent( string Content ) {

			// Trim leading and trailing whitespace
			string content = ( Content ?? "" ).Trim();
			if ( string.IsNullOrEmpty( content ) ) {
				return null;
			}

			// Trim \r\n and \r and \n\n into \n
			content = Regex.Replace( content, @"\r", "\n" );
			content = Regex.Replace( content, @"\n{2,}", "\n" );

			// Replace tabs
			content = Regex.Replace( content, @"\t+", @" " );

			// Remove @* .. *@ Razor comments
			content = Regex.Replace( content, @"@\*((?!\*@).|\n)*\*@", "" );

			{
				string step = null;
				do {
					step = content;
					// Remove CSS /* .. */ comments
					content = Regex.Replace( content, @"(<style[^>]*>(?:(?!</style>).)*)/\*(?:(?!\*/).)*\*/(.*</style>)", @"$1$2", RegexOptions.Singleline );
					// Remove JS /* .. */ comments
					content = Regex.Replace( content, @"(<script[^>]*>(?:(?!</script>).)*)/\*(?:(?!\*/).)*\*/(.*</script>)", @"$1$2", RegexOptions.Singleline );
					// Remove JS // comments
					content = Regex.Replace( content, @"(<script[^>]*>(?:(?!</script>).)*)//[^\n]*\n(.*</script>)", @"$1$2", RegexOptions.Singleline );
				} while ( step != content );
			}

			// Trim \n in lines, adding a space if there aren't tags on both sides
			content = Regex.Replace( content, ">\n<", "><" );
			content = Regex.Replace( content, "\n", " " );

			// Trim duplicate whitespace
			// FRAGILE: \s matches \n, so have to do this after removing JS comments
			content = Regex.Replace( content, @"\s{2,}", @" " );
			content = content.Trim();

			return content;
		}

	}
}