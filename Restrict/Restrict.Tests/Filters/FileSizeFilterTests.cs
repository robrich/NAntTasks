namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using NAnt.Core;
	using NAnt.Restrict.Filters;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class FileSizeFilterTests : FilterBaseTest {

		public FileSizeFilterTests()
			: base( typeof( FileSizeFilter ) ) {
		}

		[Test]
		public void MissingFileIsFalse() {

			FileSizeFilter filter = (FileSizeFilter)this.CreateInstance();

			MockFileInfo fi = notFoundFile;
			Assert.AreEqual( false, fi.Exists, "Trying to test a filter on a file that doesn't exist and the sample file does" );
			bool result = filter.Filter( fi );

			Assert.AreEqual( false, result );
		}

		[Test]
		public void DefaultWhenIsEqual() {

			FileSizeFilter filter = (FileSizeFilter)this.CreateInstance();
			Assert.AreEqual( SizeWhen.equal, filter.When );

		}

		// smaller
		[TestCase( -1, SizeWhen.equal, false )]
		[TestCase( -1, SizeWhen.greater, true )]
		[TestCase( -1, SizeWhen.eq, false )]
		[TestCase( -1, SizeWhen.gt, true )]
		[TestCase( -1, SizeWhen.less, false )]
		[TestCase( -1, SizeWhen.lt, false )]
		[TestCase( -1, SizeWhen.greaterorequal, true )]
		[TestCase( -1, SizeWhen.ge, true )]
		[TestCase( -1, SizeWhen.notequal, true )]
		[TestCase( -1, SizeWhen.ne, true )]
		[TestCase( -1, SizeWhen.lessorequal, false )]
		[TestCase( -1, SizeWhen.le, false )]
		// larger
		[TestCase( 1, SizeWhen.equal, false )]
		[TestCase( 1, SizeWhen.eq, false )]
		[TestCase( 1, SizeWhen.greater, false )]
		[TestCase( 1, SizeWhen.gt, false )]
		[TestCase( 1, SizeWhen.less, true )]
		[TestCase( 1, SizeWhen.lt, true )]
		[TestCase( 1, SizeWhen.greaterorequal, false )]
		[TestCase( 1, SizeWhen.ge, false )]
		[TestCase( 1, SizeWhen.notequal, true )]
		[TestCase( 1, SizeWhen.ne, true )]
		[TestCase( 1, SizeWhen.lessorequal, true )]
		[TestCase( 1, SizeWhen.le, true )]
		// equal
		[TestCase( 0, SizeWhen.equal, true )]
		[TestCase( 0, SizeWhen.eq, true )]
		[TestCase( 0, SizeWhen.greater, false )]
		[TestCase( 0, SizeWhen.gt, false )]
		[TestCase( 0, SizeWhen.less, false )]
		[TestCase( 0, SizeWhen.lt, false )]
		[TestCase( 0, SizeWhen.greaterorequal, true )]
		[TestCase( 0, SizeWhen.ge, true )]
		[TestCase( 0, SizeWhen.notequal, false )]
		[TestCase( 0, SizeWhen.ne, false )]
		[TestCase( 0, SizeWhen.lessorequal, true )]
		[TestCase( 0, SizeWhen.le, true )]
		public void TestIt( double SizeDiff, SizeWhen When, bool Expected ) {

			MockFileInfo fi = existingFile;

			FileSizeFilter filter = (FileSizeFilter)this.CreateInstance();
			filter.SizeInK = ( (double)fi.Length / 1024.0 ) + SizeDiff;
			filter.When = When;

			bool result = filter.Filter( fi );
			Assert.AreEqual( Expected, result );

		}

		[Test]
		public void InvalidWhenThrows() {

			MockFileInfo fi = existingFile;

			FileSizeFilter filter = (FileSizeFilter)this.CreateInstance();
			filter.When = (SizeWhen)2000;

			try {
				bool result = filter.Filter( fi );
				Assert.Fail( "Invalid SizeWhen didn't error" );
			} catch ( ValidationException ex ) {
				if ( ex.Message != "2000 is not a valid when" ) {
					throw;
				}
				// It's good
			}

		}

	}

}
