namespace NAnt.Restrict.Tests {

	#region using
	using System.Collections.Generic;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class MockFilterTests : FilterBaseTest {

		public MockFilterTests()
			: base( typeof( MockFilter ) ) {
		}

		[TestCase( true )]
		[TestCase( false )]
		public void MockWorksAsExpected( bool Value ) {

			MockFilter filter = (MockFilter)this.CreateInstance( new List<object>() { Value } );
			
			MockFileInfo fi = existingFile;

			bool result = filter.Filter( fi );

			Assert.AreEqual( Value, result );
		}

	}

}
