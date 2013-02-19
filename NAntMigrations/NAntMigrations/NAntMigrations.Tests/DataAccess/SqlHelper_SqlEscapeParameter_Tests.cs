namespace NAnt.DbMigrations.Tasks.Tests.DataAccess {
	using NAnt.DbMigrations.Tasks.DataAccess;
	using NUnit.Framework;

	[TestFixture]
	public class SqlHelper_SqlEscapeParameter_Tests : TestBase {

		[Test]
		[TestCase("normal","'normal'")]
		[TestCase("ain't","'ain''t'")]
		[TestCase("\"quoted\"","'\"quoted\"'")]
		[TestCase("percent%","'percent\\%'")]
		[TestCase("back\\slash","'back\\\\slash'")]
		public void TestSqlEscapeParameter( string Source, string ExpectedResult ) {

			// Arrange

			// Act
			SqlHelper sqlHelper = this.MockServiceLocator.Get<SqlHelper>();
			string actualResult = sqlHelper.SqlEscapeParameter( Source );

			// Assert
			Assert.That( actualResult, Is.EqualTo( ExpectedResult ) );
		}

	}
}
