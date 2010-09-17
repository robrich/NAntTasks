namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using NAnt.Restrict.Helpers;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class StringCompareTests : NonFilterBaseTest {

		[TestCase( "Some string with something", "", StringComparison.InvariantCultureIgnoreCase, false )]
		[TestCase( "", "Some string with something", StringComparison.InvariantCultureIgnoreCase, false )]
		[TestCase( "Some string with something", "Some string", StringComparison.InvariantCultureIgnoreCase, true )]
		[TestCase( "Some string with something", "Some string", StringComparison.InvariantCulture, true )]
		[TestCase( "Some string with something", "some string", StringComparison.InvariantCulture, false )]
		public void TestContains( string Search, string Find, StringComparison sc, bool ExpectedResult ) {

			bool result = Search.Contains( Find, sc );
			Assert.AreEqual( ExpectedResult, result );

		} 

	}

}
