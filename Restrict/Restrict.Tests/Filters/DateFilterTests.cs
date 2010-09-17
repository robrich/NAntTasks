namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using NAnt.Core;
	using NAnt.Restrict.Filters;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class DateFilterTests : FilterBaseTest {

		public DateFilterTests()
			: base( typeof( DateFilter ) ) {
		}

		private DateTime EPOCH {
			get { return new DateTime( 1970, 1, 1 ); }
		}

		[Test]
		public void MissingFileIsFalse() {

			DateFilter filter = (DateFilter)this.CreateInstance();

			MockFileInfo fi = notFoundFile;
			bool result = filter.Filter( fi );

			Assert.AreEqual( false, result );
		}

		[Test]
		public void DefaultWhenIsEqual() {

			DateFilter filter = (DateFilter)this.CreateInstance();
			Assert.AreEqual( TimeWhen.equal, filter.When );

		}

		// after
		[TestCase( 1, TimeWhen.before, true )]
		[TestCase( 1, TimeWhen.equal, false )]
		[TestCase( 1, TimeWhen.after, false )]
		// before
		[TestCase( -1, TimeWhen.before, false )]
		[TestCase( -1, TimeWhen.equal, false )]
		[TestCase( -1, TimeWhen.after, true )]
		// equal: set this way because granularity is 0
		[TestCase( 0, TimeWhen.before, false )]
		[TestCase( 0, TimeWhen.equal, true )]
		[TestCase( 0, TimeWhen.after, false )]
		public void TestIt( double DaysDiff, TimeWhen When, bool Expected ) {

			MockFileInfo fi = existingFile;

			DateFilter filter = (DateFilter)this.CreateInstance();
			filter.DateTime = fi.LastWriteTime.AddDays( DaysDiff );
			filter.When = When;
			filter.Granularity = 0; // So "equal" doesn't mean "within 1 second" and thus "0 seconds is before because it's before 1 sec after"
			
			bool result = filter.Filter( fi );
			Assert.AreEqual( Expected, result );

		}

		// after
		[TestCase( 1, TimeWhen.before, true )]
		[TestCase( 1, TimeWhen.equal, false )]
		[TestCase( 1, TimeWhen.after, false )]
		// before
		[TestCase( -1, TimeWhen.before, false )]
		[TestCase( -1, TimeWhen.equal, false )]
		[TestCase( -1, TimeWhen.after, true )]
		// equal: set this way because granularity is 0
		[TestCase( 0, TimeWhen.before, false )]
		[TestCase( 0, TimeWhen.equal, true )]
		[TestCase( 0, TimeWhen.after, false )]
		public void TestItFromEpoc( double DaysDiff, TimeWhen When, bool Expected ) {

			MockFileInfo fi = existingFile;

			DateFilter filter = (DateFilter)this.CreateInstance();
			filter.MillisecondsSinceEpoch = ( fi.LastWriteTime.AddDays( DaysDiff ) - this.EPOCH ).TotalMilliseconds;
			filter.When = When;
			filter.Granularity = 0; // So "equal" doesn't mean "within 1 second" and thus "0 seconds is before because it's before 1 sec after"
			
			bool result = filter.Filter( fi );
			Assert.AreEqual( Expected, result );

		}

		// before
		[TestCase( TimeWhen.before, -1, 0, false )] // Just before
		[TestCase( TimeWhen.before, -1, 1, false )] // On the border fails
		[TestCase( TimeWhen.before, -1, 2, true )] // Just after
		// after
		[TestCase( TimeWhen.after, 1, 0, false )] // Just before
		[TestCase( TimeWhen.after, 1, 1, false )] // On the border fails
		[TestCase( TimeWhen.after, 1, 2, true )] // Just after
		// equal
		[TestCase( TimeWhen.equal, 2, 1, false )] // Just before
		[TestCase( TimeWhen.equal, 1, 1, true )] // On the border succeeds
		[TestCase( TimeWhen.equal, 0, 1, true )] // Inside the granularity window
		[TestCase( TimeWhen.equal, -1, 1, true )] // On the border succeeds
		[TestCase( TimeWhen.equal, -2, 1, false )] // Just after
		public void GranularityTest( TimeWhen When, int Seconds, int Granularity, bool Expected ) {

			MockFileInfo fi = existingFile;

			DateFilter filter = (DateFilter)this.CreateInstance();
			filter.MillisecondsSinceEpoch = ( fi.LastWriteTime.AddSeconds( Seconds ) - this.EPOCH ).TotalMilliseconds;
			filter.When = When;
			filter.Granularity = Granularity * 1000; // Because Granularity is in Milliseconds
			
			bool result = filter.Filter( fi );
			Assert.AreEqual( Expected, result );

		}

		[Test]
		public void InvalidWhenThrows() {

			MockFileInfo fi = existingFile;

			DateFilter filter = (DateFilter)this.CreateInstance();
			filter.When = (TimeWhen)2000;

			try {
				bool result = filter.Filter( fi );
				Assert.Fail( "Invalid TimeWhen didn't error" );
			} catch ( ValidationException ex ) {
				if ( ex.Message != "2000 is not a valid when" ) {
					throw;
				}
				// It's good
			}

		}

		[Test]
		public void NoDateOrMillsIsEpoch() {

			MockFileInfo fi = new MockFileInfo( "mockfile" ) {
				LastWriteTime_Mock = this.EPOCH
			};

			DateFilter filter = (DateFilter)this.CreateInstance();
			filter.Granularity = 0;
			filter.When = TimeWhen.equal;

			// We can't test the date it uses, but we can test if it's equal to something
			bool result = filter.Filter( fi );
			Assert.AreEqual( true, result );

		}

		[Test]
		public void BothDateAndMillsThrows() {

			MockFileInfo fi = this.existingFile;

			DateFilter filter = (DateFilter)this.CreateInstance();
			filter.MillisecondsSinceEpoch = 1000;
			filter.DateTime = new DateTime( 2000, 1, 1 );

			try {
				bool result = filter.Filter( fi );
				Assert.Fail( "Both DateTime and Mills didn't error" );
			} catch ( ValidationException ex ) {
				if ( !ex.Message.Contains( "exactly one datetime or mills" ) ) {
					throw;
				}
				// It's good
			}

		}

	}

}
