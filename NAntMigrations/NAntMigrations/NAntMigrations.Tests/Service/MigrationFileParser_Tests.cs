namespace NAnt.DbMigrations.Tasks.Tests.Service {
	using System;
	using NAnt.DbMigrations.Tasks.Service;
	using NUnit.Framework;

	[TestFixture]
	public class MigrationFileParser_Tests : TestBase {

		[Test]
		public void NoDown_ReturnsEmpty() {

			// Arrange
			string expectedDownMigration = null;
			string source = @"SELECT * FROM dbo.Something
GO
";
			string expectedTheRest = source;

			// Act and Assert
			this.RunTest( source, expectedDownMigration, expectedTheRest );
		}

		[Test]
		public void DownInSentance_ReturnsEmpty() {

			// Arrange
			string expectedDownMigration = null;
			string source = @"SELECT * FROM dbo.Something
GO
/*
Down by the river
DELETE * FROM dbo.Something
*/
";
			string expectedTheRest = @"SELECT * FROM dbo.Something
GO
";

			// Act and Assert
			this.RunTest( source, expectedDownMigration, expectedTheRest );
		}

		[Test]
		public void DownOnOwnLine_ReturnsContent() {

			// Arrange
			string expectedDownMigration = "DELETE * FROM dbo.Something";
			string source = @"SELECT * FROM dbo.Something
GO
/*
DOWN
" + expectedDownMigration + @"
*/
";
			string expectedTheRest = @"SELECT * FROM dbo.Something
GO
";

			// Act and Assert
			this.RunTest( source, expectedDownMigration, expectedTheRest );
		}

		[Test]
		public void DownWithSpaces_ReturnsContent() {

			// Arrange
			string expectedDownMigration = "DELETE * FROM dbo.Something";
			string source = @"SELECT * FROM dbo.Something
GO
/*  DOWN  
" + expectedDownMigration + @"
*/
";
			string expectedTheRest = @"SELECT * FROM dbo.Something
GO
";

			// Act and Assert
			this.RunTest( source, expectedDownMigration, expectedTheRest );
		}

		[Test]
		public void Null_ReturnsEmpty() {

			// Arrange
			string expectedDownMigration = null;
			string source = null;
			string expectedTheRest = source;

			// Act and Assert
			this.RunTest( source, expectedDownMigration, expectedTheRest );
		}

		private void RunTest( string Source, string ExpectedDownScript, string ExpectedTheRest ) {
			
			// Act
			MigrationFileParser migrationFileParser = this.MockServiceLocator.Get<MigrationFileParser>();
			Tuple<string, string> actualContent = migrationFileParser.GetDownScript( Source );

			// Assert
			Assert.That( actualContent, Is.Not.Null );
			Assert.That( actualContent.Item1, Is.EqualTo( ExpectedDownScript ) );
			Assert.That( actualContent.Item2, Is.EqualTo( ExpectedTheRest ) );
		}

	}
}
