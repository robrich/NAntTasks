namespace NAnt.Restrict.Tests {

	#region using
	using System.Collections.Generic;
	using NAnt.Restrict.Filters;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class MajorityFilterTests : FilterNestedBaseTest {

		public MajorityFilterTests()
			: base( typeof( MajorityFilter ) ) {
		}

		[TestCase( true, true )]
		[TestCase( false, false )]
		public void OneNestedFilterTieTrueTests( bool NestedValue, bool ExpectedValue ) {
			base.RunOneNestedFilterTests( NestedValue, ExpectedValue, new List<object>() { true } );
		}

		[TestCase( true, true )]
		[TestCase( false, false )]
		public void OneNestedFilterTieFalseTests( bool NestedValue, bool ExpectedValue ) {
			base.RunOneNestedFilterTests( NestedValue, ExpectedValue, new List<object>() { false } );
		}

		[TestCase( true, true, true )]
		[TestCase( true, false, true )]
		[TestCase( false, false, false )]
		public void TwoNestedFilterTieTrueTests( bool NestedValue1, bool NestedValue2, bool ExpectedValue ) {
			base.RunTwoNestedFilterTests( NestedValue1, NestedValue2, ExpectedValue, new List<object>() { true } );
		}

		[TestCase( true, true, true )]
		[TestCase( true, false, false )]
		[TestCase( false, false, false )]
		public void TwoNestedFilterTieFalseTests( bool NestedValue1, bool NestedValue2, bool ExpectedValue ) {
			base.RunTwoNestedFilterTests( NestedValue1, NestedValue2, ExpectedValue, new List<object>() { false } );
		}

		[TestCase( true, true, true, true )]
		[TestCase( true, true, false, true )]
		[TestCase( true, false, false, false )]
		[TestCase( false, false, false, false )]
		public void ThreeNestedFilterTieTrueTests( bool NestedValue1, bool NestedValue2, bool NestedValue3, bool ExpectedValue ) {
			base.RunThreeNestedFilterTests( NestedValue1, NestedValue2, NestedValue3, ExpectedValue, new List<object>() { true } );
		}

		[TestCase( true, true, true, true )]
		[TestCase( true, true, false, true )]
		[TestCase( true, false, false, false )]
		[TestCase( false, false, false, false )]
		public void ThreeNestedFilterTieFalseTests( bool NestedValue1, bool NestedValue2, bool NestedValue3, bool ExpectedValue ) {
			base.RunThreeNestedFilterTests( NestedValue1, NestedValue2, NestedValue3, ExpectedValue, new List<object>() { false } );
		}

		[TestCase( true, true, true, true, true )]
		[TestCase( true, true, true, false, true )]
		[TestCase( true, true, false, false, false )]
		[TestCase( true, false, false, false, false )]
		[TestCase( false, false, false, false, false )]
		public void FourNestedFilterTieTrueTests( bool NestedValue1, bool NestedValue2, bool NestedValue3, bool NestedValue4, bool ExpectedValue ) {
			base.RunFourNestedFilterTests( NestedValue1, NestedValue2, NestedValue3, NestedValue4, ExpectedValue, new List<object>() { false } );
		}

		[TestCase( true, true, true, true, true )]
		[TestCase( true, true, true, false, true )]
		[TestCase( true, true, false, false, false )]
		[TestCase( true, false, false, false, false )]
		[TestCase( false, false, false, false, false )]
		public void FourNestedFilterTieFalseTests( bool NestedValue1, bool NestedValue2, bool NestedValue3, bool NestedValue4, bool ExpectedValue ) {
			base.RunFourNestedFilterTests( NestedValue1, NestedValue2, NestedValue3, NestedValue4, ExpectedValue, new List<object>() { false } );
		}

		[Test]
		public void AllowTieDefaultsTrue() {

			MajorityFilter filter = (MajorityFilter)this.CreateInstance();
			Assert.IsTrue( filter.AllowTie );

		}

	}

}
