namespace NAnt.Restrict.Tests {

	#region using
	using NAnt.Restrict.Filters;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class OrFilterTests : FilterNestedBaseTest {

		public OrFilterTests()
			: base( typeof( OrFilter ) ) {
		}

		[TestCase( true, true )]
		[TestCase( false, false )]
		public void OneNestedFilterTests( bool NestedValue, bool ExpectedValue ) {
			base.RunOneNestedFilterTests( NestedValue, ExpectedValue );
		}

		[TestCase( true, true, true )]
		[TestCase( true, false, true )]
		[TestCase( false, false, false )]
		public void TwoNestedFilterTests( bool NestedValue1, bool NestedValue2, bool ExpectedValue ) {
			base.RunTwoNestedFilterTests( NestedValue1, NestedValue2, ExpectedValue );
		}

		[TestCase( true, true, true, true )]
		[TestCase( true, true, false, true )]
		[TestCase( true, false, false, true )]
		[TestCase( false, false, false, false )]
		public void ThreeNestedFilterTests( bool NestedValue1, bool NestedValue2, bool NestedValue3, bool ExpectedValue ) {
			base.RunThreeNestedFilterTests( NestedValue1, NestedValue2, NestedValue3, ExpectedValue );
		}

	}

}
