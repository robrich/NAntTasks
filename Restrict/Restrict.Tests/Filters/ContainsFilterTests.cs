namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using NAnt.Core;
	using NAnt.Restrict.Filters;
	using NAnt.Restrict.Helpers;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class ContainsFilterTests : FilterBaseTest {

		public ContainsFilterTests()
			: base( typeof( ContainsFilter ) ) {
		}

		private const string filecontent = "This file contains some stuff\nIt has multiple lines in it\nSome lines are irrelevant or overly verbose";

		[TestCase( "multiple lines", true, true )]
		[TestCase( "Multiple lines", false, true )]
		[TestCase( "Multiple lines", true, false )]
		[TestCase( "This text isn't in the file", true, false )]
		public void StringFilterTest( string StringFilter, bool CaseSensitive, bool ExpectedResult ) {

			ContainsFilter filter = (ContainsFilter)this.CreateInstance();
			filter.CaseSensitive = CaseSensitive;
			filter.StringFilter = StringFilter;

			MockFileInfo fi = this.existingFile;
			fi.Contents_Mock = filecontent;

			bool result = filter.Filter( fi );

			Assert.AreEqual( ExpectedResult, result );
		}

		[TestCase( "multiple lines", true, true )]
		[TestCase( "Multiple lines", false, true )]
		[TestCase( "This text isn't in the file", true, false )]
		[TestCase( "^It has", true, true )]
		[TestCase( "^it has", false, true )]
		[TestCase( "^It has", false, true )]
		public void RegexFilterTest( string RegexFilter, bool CaseSensitive, bool ExpectedResult ) {

			ContainsFilter filter = (ContainsFilter)this.CreateInstance();
			filter.CaseSensitive = CaseSensitive;
			filter.RegexFilter = RegexFilter;

			MockFileInfo fi = this.existingFile;
			fi.Contents_Mock = filecontent;

			bool result = filter.Filter( fi );

			Assert.AreEqual( ExpectedResult, result );
		}

		[Test]
		public void InvalidRegexThrowsException() {

			ContainsFilter filter = (ContainsFilter)this.CreateInstance();
			filter.RegexFilter = "(";

			MockFileInfo fi = this.existingFile;
			fi.Contents_Mock = filecontent;

			try {
				bool result = filter.Filter( fi );
				Assert.Fail( "An exception wasn't thrown with an invalid regex" );
			} catch ( BuildException ex ) {
				if ( ex == null || ex.InnerException == null
					|| !ex.Message.Contains( "regex", StringComparison.InvariantCultureIgnoreCase ) || ex.InnerException.GetType() != typeof( ArgumentException ) ) {
					throw;
				} else {
					// It passed
				}
			}

		}

		[Test]
		public void BlankFilenameReturnsFalse() {

			ContainsFilter filter = (ContainsFilter)this.CreateInstance();
			filter.StringFilter = "something";

			MockFileInfo fi = new MockFileInfo( "" );

			bool result = filter.Filter( fi );
			Assert.AreEqual( false, result, "A blank filename didn't fail the test" );

		}

		[Test]
		public void MissingFileReturnsFalse() {

			ContainsFilter filter = (ContainsFilter)this.CreateInstance();
			filter.StringFilter = "something";

			MockFileInfo fi = this.notFoundFile;

			bool result = filter.Filter( fi );
			Assert.AreEqual( false, result, "A missing file didn't fail the test" );

		}

		[Test]
		public void NoFilenameOrRegexThrows() {

			ContainsFilter filter = (ContainsFilter)this.CreateInstance();
			// No StringFilter, no RegexFilter

			MockFileInfo fi = this.existingFile;
			
			try {
				bool result = filter.Filter( fi );
				Assert.Fail( "An exception wasn't thrown with no string or regex" );
			} catch ( BuildException ex ) {
				if ( !ex.Message.Contains( "requires exactly one", StringComparison.InvariantCultureIgnoreCase ) ) {
					throw;
				} else {
					// It passed
				}
			}

		}

	}

}
