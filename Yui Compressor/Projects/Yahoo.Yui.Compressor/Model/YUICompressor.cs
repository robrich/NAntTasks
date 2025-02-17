using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Yahoo.Yui.Compressor
{
    public static class YUICompressor
    {
        #region Private Extension Methods

        private static string RemoveCommentBlocks(this string input)
        {
            int startIndex = 0;
            int endIndex = 0;
            bool iemac = false;
            bool preserve = false;
            
            startIndex = input.IndexOf(@"/*",
                startIndex,
                StringComparison.OrdinalIgnoreCase);
            while (startIndex >= 0)
            {
                preserve = input.Length > startIndex + 2 &&
                    input[startIndex + 2] == '!';

                endIndex = input.IndexOf(@"*/",
                    startIndex + 2,
                    StringComparison.OrdinalIgnoreCase);

                if (endIndex < 0)
                {
                    if (!preserve)
                    {
                        input = input.RemoveRange(startIndex, input.Length);
                    }
                }
                else if (endIndex >= startIndex + 2)
                {
                    if (input[endIndex - 1] == '\\')
                    {
                        startIndex = endIndex + 2;
                        iemac = true;
                    }
                    else if (iemac)
                    {
                        startIndex = endIndex + 2;
                        iemac = false;
                    }
                    else if (!preserve)
                    {
                        input = input.RemoveRange(startIndex, endIndex + 2);
                    }
                    else
                    {
                        startIndex = endIndex + 2;
                    }
                }

                startIndex = input.IndexOf(@"/*",
                    startIndex,
                    StringComparison.OrdinalIgnoreCase);
            }

            return input;
        }

        private static string ShortenRgbColors(this string css)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Regex pattern = new Regex("rgb\\s*\\(\\s*([0-9,\\s]+)\\s*\\)");
            Match match = pattern.Match(css);

            int index = 0;
            while (match.Success)
            {
                int value;
                string[] colors = match.Groups[1].Value.Split(',');
                StringBuilder hexcolor = new StringBuilder("#");


                foreach (string color in colors)
                {
                    if (!Int32.TryParse(color,
                        out value))
                    {
                        value = 0;
                    }

                    if (value < 16)
                    {
                        hexcolor.Append("0");
                    }

                    hexcolor.Append(value.ToHexString());
                }

                index = match.AppendReplacement(stringBuilder,
                    css,
                    hexcolor.ToString(),
                    index);
                match = match.NextMatch();
            }

            stringBuilder.AppendTail(css,
                index);

            return stringBuilder.ToString();
        }

        private static string ShortenHexColors(this string css)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Regex pattern = new Regex("([^\"'=\\s])(\\s*)#([0-9a-fA-F])([0-9a-fA-F])([0-9a-fA-F])([0-9a-fA-F])([0-9a-fA-F])([0-9a-fA-F])");
            Match match = pattern.Match(css);

            int index = 0;
            while (match.Success)
            {
                if (match.Groups[3].Value.EqualsIgnoreCase(match.Groups[4].Value) &&
                    match.Groups[5].Value.EqualsIgnoreCase(match.Groups[6].Value) &&
                    match.Groups[7].Value.EqualsIgnoreCase(match.Groups[8].Value))
                {
                    var replacement = String.Concat(match.Groups[1].Value,
                        match.Groups[2].Value,
                        "#", match.Groups[3].Value,
                        match.Groups[5].Value,
                        match.Groups[7].Value);
                    index = match.AppendReplacement(stringBuilder,
                        css,
                        replacement,
                        index);
                }
                else
                {
                    index = match.AppendReplacement(stringBuilder,
                        css,
                        match.Value,
                        index);
                }

                match = match.NextMatch();
            }

            stringBuilder.AppendTail(css,
                index);

            return stringBuilder.ToString();
        }

        private static string RemovePrecedingSpaces(this string css)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Regex pattern = new Regex("(^|\\})(([^\\{:])+:)+([^\\{]*\\{)");
            Match match = pattern.Match(css);

            int index = 0;
            while (match.Success)
            {
                string s = match.Value;
                s = s.RegexReplace(":", "___PSEUDOCLASSCOLON___");

                index = match.AppendReplacement(stringBuilder,
                    css,
                    s,
                    index);
                match = match.NextMatch();
            }
            stringBuilder.AppendTail(css,
                index);

            string result = stringBuilder.ToString();
            result = result.RegexReplace("\\s+([!{};:>+\\(\\)\\],])", "$1");
			// Put the space back in some cases, to support stuff like
			// @media screen and (-webkit-min-device-pixel-ratio:0){
			// https://github.com/isaacs/yuicompressor/commit/0e290b8e05392f19bccc2f7d524725a87739e6d6
			result = result.RegexReplace("(@media[^{]*[^\\s])\\(", "$1 (");
            result = result.RegexReplace("___PSEUDOCLASSCOLON___", ":");

            return result;
        }

        private static string BreakLines(this string css,
            int columnWidth)
        {
            int i = 0;
            int start = 0;

            StringBuilder stringBuilder = new StringBuilder(css);
            while (i < stringBuilder.Length)
            {
                char c = stringBuilder[i++];
                if (c == '}' &&
                    i - start > columnWidth)
                {
                    stringBuilder.Insert(i, '\n');
                    start = i;
                }
            }

            return stringBuilder.ToString();
        }

        #endregion

        public static string Compress(string css,
            int columnWidth)
        {
            if (string.IsNullOrEmpty(css))
            {
                throw new ArgumentNullException("css");
            }

            // Safety check the other arguments.
            if (columnWidth < 0)
            {
                columnWidth = 0;
            }

            // Now compress the css!
            css = css.RemoveCommentBlocks();
            css = css.RegexReplace("\\s+", " ");
            css = css.RegexReplace("\"\\\\\"}\\\\\"\"", "___PSEUDOCLASSBMH___");
            css = css.RemovePrecedingSpaces();
            css = css.RegexReplace("([!{}:;>+\\(\\[,])\\s+", "$1");
            css = css.RegexReplace("([^;\\}])}", "$1;}");
            css = css.RegexReplace("([\\s:])(0)(px|em|%|in|cm|mm|pc|pt|ex)", "$1$2");
            css = css.RegexReplace(":0 0 0 0;", ":0;");
            css = css.RegexReplace(":0 0 0;", ":0;");
            css = css.RegexReplace(":0 0;", ":0;");
            css = css.RegexReplace("background-position:0;", "background-position:0 0;");
            css = css.RegexReplace("(:|\\s)0+\\.(\\d+)", "$1.$2");
            css = css.ShortenRgbColors();
            css = css.ShortenHexColors();
            css = css.RegexReplace("[^\\}]+\\{;\\}", "");

            if (columnWidth > 0)
            {
                css = css.BreakLines(columnWidth);
            }

            css = css.RegexReplace("___PSEUDOCLASSBMH___", "\"\\\\\"}\\\\\"\"");

            // Replace multiple semi-colons in a row by a single one
            // See SF bug #1980989
            css = css.Replace(";;+", ";");

            css = css.Trim();

            return css;
        }
    }
}