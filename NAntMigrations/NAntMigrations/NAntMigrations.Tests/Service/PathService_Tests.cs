namespace NAnt.DbMigrations.Tasks.Tests.Service {
	using NAnt.DbMigrations.Tasks.Service;
	using NUnit.Framework;

	[TestFixture]
	public class PathService_Tests : TestBase {

		private const string BASE_DIRECTORY = @"C:\Users\NUnit\Documents";

		[Test]
		[TestCase( @"path/to/file.js", @"C:\Users\NUnit\Documents\path\to\file.js" )]
		[TestCase( @"c:\path\to\file.js", @"c:\path\to\file.js" )]
		[TestCase( @"\\machine\path\to\file.js", @"\\machine\path\to\file.js" )]
		[TestCase( @"..\path\to\file.js", @"C:\Users\NUnit\path\to\file.js" )]
		public void TestNormalizePath( string Source, string ExpectedResult ) {

			// Arrange

			// Act
			PathService pathService = this.MockServiceLocator.Get<PathService>();
			string actualResult = pathService.NormalizePath( BASE_DIRECTORY, Source );

			// Assert
			Assert.That( actualResult, Is.EqualTo( ExpectedResult ) );
		}

	}
}
