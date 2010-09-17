namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using System.Collections.Generic;
	using NAnt.Restrict.Filters;
	using NUnit.Framework;

	#endregion

	public class FilterNestedBaseTest : FilterBaseTest {

		public FilterNestedBaseTest( Type TestType )
			: base( TestType ) {
		}

		protected void RunOneNestedFilterTests( bool NestedValue1, bool ExpectedValue, List<object> ConstructorParameters = null ) {

			List<FilterBase> filters = new List<FilterBase>() {
				new MockFilter( NestedValue1 )
			};

			this.RunFilterTest( filters, 1, ExpectedValue, ConstructorParameters );
		}

		protected void RunTwoNestedFilterTests( bool NestedValue1, bool NestedValue2, bool ExpectedValue, List<object> ConstructorParameters = null ) {

			List<FilterBase> filters = new List<FilterBase>() {
				new MockFilter( NestedValue1 ),
				new MockFilter( NestedValue2 )
			};

			this.RunFilterTest( filters, 2, ExpectedValue, ConstructorParameters );
		}

		protected void RunThreeNestedFilterTests( bool NestedValue1, bool NestedValue2, bool NestedValue3, bool ExpectedValue, List<object> ConstructorParameters = null ) {

			List<FilterBase> filters = new List<FilterBase>() {
				new MockFilter( NestedValue1 ),
				new MockFilter( NestedValue2 ),
				new MockFilter( NestedValue3 )
			};

			this.RunFilterTest( filters, 3, ExpectedValue, ConstructorParameters );
		}

		protected void RunFourNestedFilterTests( bool NestedValue1, bool NestedValue2, bool NestedValue3, bool NestedValue4, bool ExpectedValue, List<object> ConstructorParameters = null ) {

			List<FilterBase> filters = new List<FilterBase>() {
				new MockFilter( NestedValue1 ),
				new MockFilter( NestedValue2 ),
				new MockFilter( NestedValue3 ),
				new MockFilter( NestedValue4 )
			};

			this.RunFilterTest( filters, 4, ExpectedValue, ConstructorParameters );
		}

		protected void RunFilterTest( List<FilterBase> Filters, int ExpectedFilterCount, bool ExpectedValue, List<object> ConstructorParameters = null ) {

			List<object> ctorParams = new List<object> {
				Filters
			};
			if ( ConstructorParameters != null && ConstructorParameters.Count > 0 ) {
				ctorParams.AddRange( ConstructorParameters );
			}

			FilterNestedBase filter = (FilterNestedBase)this.CreateInstance( ctorParams );

			Assert.AreEqual( ExpectedFilterCount, filter.NestedFilterCount );

			MockFileInfo fi = existingFile;

			filter.NestedInitialize();

			bool result = filter.Filter( fi );

			Assert.AreEqual( ExpectedValue, result );
		}

	}

}
