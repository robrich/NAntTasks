namespace NAnt.Restrict.Tests {

	#region using
	using NAnt.Core;
	using System.Collections.Generic;
	using NAnt.Restrict.Filters;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class FilterNestedBaseTests : FilterBaseTest {

		// We're not really testing And, but we need something that derives from FilterNestedBase since FilterNestedBase is abstract
		public FilterNestedBaseTests()
			: base( typeof( AndFilter ) ) {
		}

		[Test]
		public void NoInnerFiltersFailsToInitialize() {

			FilterNestedBase filter = (FilterNestedBase)this.CreateInstance();

			Assert.AreEqual( 0, filter.NestedFilterCount );

			MockFileInfo fi = this.existingFile;

			try {
				filter.NestedInitialize();
				Assert.Fail( "A FilterNestedBase with no children didn't fail" );
			} catch ( BuildException ex ) {
				if ( !ex.Message.Contains( "has no nested" ) ) {
					throw;
				} else {
					// It passed
				}
			}

		}

		[Test]
		public void NestedInnerFiltersInitialize() {

			bool state = true;
			MockNestedFilter nest = new MockNestedFilter(
				state
			);

			List<object> ctorParams = new List<object> {
				new List<FilterBase>() {
					nest
				}
			};

			FilterNestedBase filter = (FilterNestedBase)this.CreateInstance( ctorParams );

			Assert.AreEqual( 1, filter.NestedFilterCount );

			MockFileInfo fi = this.existingFile;

			filter.NestedInitialize();
			Assert.AreEqual( true, nest.NestedInitializeCalled );

		}

	}

}
