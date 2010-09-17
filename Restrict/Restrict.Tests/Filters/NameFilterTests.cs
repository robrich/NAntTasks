namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using NAnt.Core;
	using NAnt.Restrict.Filters;
	using NAnt.Restrict.Helpers;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class NameFilterTests : FilterBaseTest {

		public NameFilterTests()
			: base( typeof( NameFilter ) ) {
		}

		private const string filename = @"C:\Users\Username\Documents\MyFile.xls";

		[TestCase( "hernando", true, true, false )]
		[TestCase( "MyFile", true, true, true )]
		[TestCase( "myfile", true, true, false )]
		[TestCase( "myfile", false, true, true )]
		[TestCase( @"Users\Username", false, false, true )]
		[TestCase( @"Users\Username", false, true, false )]
		[TestCase( @"Users/Username", false, false, false )]
		[TestCase( @"Users/Username", false, true, true )]
		public void FilterByString( string StringFilter, bool CaseSensitive, bool HandleDirSep, bool ExpectedResult ) {

			NameFilter filter = (NameFilter)this.CreateInstance();
			filter.CaseSensitive = CaseSensitive;
			filter.HandleDirSep = HandleDirSep;
			filter.StringFilter = StringFilter;

			MockFileInfo fi = new MockFileInfo( filename );

			bool result = filter.Filter( fi );

			Assert.AreEqual( ExpectedResult, result );
		}

		[TestCase( "hernando", true, true, false )]
		[TestCase( "MyFile", true, true, true )]
		[TestCase( "myfile", true, true, false )]
		[TestCase( "myfile", false, true, true )]
		[TestCase( @"Users\\Username", false, false, true )]
		[TestCase( @"Users\\Username", false, true, false )]
		[TestCase( @"Users/Username", false, false, false )]
		[TestCase( @"Users/Username", false, true, true )]
		[TestCase( "Users.*File\\.xls$", true, true, true )]
		[TestCase( "users.*file\\.xls$", true, true, false )]
		[TestCase( "users.*file\\.xls$", false, true, true )]
		[TestCase( "^notfound$", true, true, false )]
		public void FilterByRegex( string RegexFilter, bool CaseSensitive, bool HandleDirSep, bool ExpectedResult ) {

			NameFilter filter = (NameFilter)this.CreateInstance();
			filter.CaseSensitive = CaseSensitive;
			filter.HandleDirSep = HandleDirSep;
			filter.RegexFilter = RegexFilter;

			MockFileInfo fi = new MockFileInfo( filename );

			bool result = filter.Filter( fi );

			Assert.AreEqual( ExpectedResult, result );
		}

		[Test]
		public void InvalidRegexThrowsException() {

			NameFilter filter = (NameFilter)this.CreateInstance();
			filter.RegexFilter = "(";

			MockFileInfo fi = new MockFileInfo( filename );

			try {
				bool result = filter.Filter( fi );
				Assert.Fail( "An exception wasn't thrown with an invalid regex" );
			} catch ( BuildException ex ) {
				if ( ex == null || ex.InnerException == null 
					|| !ex.Message.Contains( "regex", StringComparison.InvariantCultureIgnoreCase ) || ex.InnerException.GetType() != typeof(ArgumentException) ) {
					throw;
				} else {
					// It passed
				}
			}

		}

		[Test]
		public void BlankFilenameReturnsFalse() {

			NameFilter filter = (NameFilter)this.CreateInstance();
			filter.StringFilter = "something";

			MockFileInfo fi = new MockFileInfo( "" );
			
			bool result = filter.Filter( fi );
			Assert.AreEqual( false, result, "A blank filename didn't fail the test" );

		}

		[Test]
		public void NoFilenameOrRegexThrows() {

			NameFilter filter = (NameFilter)this.CreateInstance();
			// No StringFilter, no RegexFilter

			MockFileInfo fi = new MockFileInfo( filename );

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
