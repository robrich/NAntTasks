namespace NAnt.Restrict.Tests {

	#region using
	using NAnt.Restrict.Filters;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class ExistsFilterTests : FilterBaseTest {

		public ExistsFilterTests()
			: base( typeof( ExistsFilter ) ) {
		}

		[Test]
		public void ExistingFilePasses() {

			ExistsFilter filter = (ExistsFilter)this.CreateInstance();

			MockFileInfo fi = existingFile;
			Assert.AreEqual( true, fi.Exists, "Trying to test a filter on a file that exists and the sample file doesn't" );

			bool result = filter.Filter( fi );

			Assert.AreEqual( true, result );
		}

		[Test]
		public void MissingFileFails() {

			ExistsFilter filter = (ExistsFilter)this.CreateInstance();

			MockFileInfo fi = notFoundFile;
			Assert.AreEqual( false, fi.Exists, "Trying to test a filter on a file that doesn't exist and the sample file does" );

			bool result = filter.Filter( fi );

			Assert.AreEqual( false, result );
		}

	}

}
