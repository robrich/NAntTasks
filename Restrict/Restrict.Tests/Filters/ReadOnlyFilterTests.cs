namespace NAnt.Restrict.Tests {

	#region using
	using NAnt.Restrict.Filters;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class ReadOnlyFilterTests : FilterBaseTest {

		public ReadOnlyFilterTests()
			: base( typeof( ReadOnlyFilter ) ) {
		}

		private const string filename = "ReadOnly_ContentFile.txt";

		[TestCase( true, true )]
		[TestCase( false, false )]
		public void ReadOnlyTesting( bool ReadOnlyState, bool ExpectedResult ) {

			ReadOnlyFilter filter = (ReadOnlyFilter)this.CreateInstance();

			MockFileInfo fi = new MockFileInfo( filename );
			Assert.AreEqual( true, fi.Exists, "Trying to test a filter on a file that exists and the sample file doesn't" );

			fi.IsReadOnly_Mock = ReadOnlyState;

			bool result = filter.Filter( fi );

			Assert.AreEqual( ExpectedResult, result );

		}

		[Test]
		public void BlankFilenameReturnsFalse() {

			ReadOnlyFilter filter = (ReadOnlyFilter)this.CreateInstance();

			MockFileInfo fi = new MockFileInfo( "" );

			bool result = filter.Filter( fi );
			Assert.AreEqual( false, result, "A blank filename didn't fail the test" );

		}

		[Test]
		public void MissingFileReturnsFalse() {

			ReadOnlyFilter filter = (ReadOnlyFilter)this.CreateInstance();

			MockFileInfo fi = this.notFoundFile;

			bool result = filter.Filter( fi );
			Assert.AreEqual( false, result, "A missing file didn't fail the test" );

		}

	}

}
