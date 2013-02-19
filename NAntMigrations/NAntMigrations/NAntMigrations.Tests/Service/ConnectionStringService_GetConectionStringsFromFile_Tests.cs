namespace NAnt.DbMigrations.Tasks.Tests.Service {
	using System.Collections.Generic;
	using System.Xml.Linq;
	using NAnt.DbMigrations.Tasks.Service;
	using NUnit.Framework;

	[TestFixture]
	public class ConnectionStringService_GetConectionStringsFromFile_Tests : TestBase {

		[Test]
		public void FindsOneConnectionString() {

			// Arrange
			string expectedString = "This is the connection string";
			string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
				<configuration>
					<connectionStrings>
						<clear />
						<add name=""ConnectionString""
							connectionString=""" + expectedString + @"""
							providerName=""System.Data.SqlClient"" />
					</connectionStrings>
				</configuration>";
			XElement xelement = XElement.Parse( xml );

			// Act
			ConnectionStringService connectionStringService = this.MockServiceLocator.Get<ConnectionStringService>();
			List<string> actual = connectionStringService.GetConnectionStringsFromFile( xelement );

			// Assert
			Assert.That( actual, Is.Not.Null );
			Assert.That( actual.Count, Is.EqualTo( 1 ) );
			Assert.That( actual[0], Is.EqualTo( expectedString ) );
		}

		[Test]
		public void FindsOneConnectionStringInPartialFile() {

			// Arrange
			string expectedString = "This is the connection string";
			string xml = @"<connectionStrings>
					<clear />
					<add name=""ConnectionString""
						connectionString=""" + expectedString + @"""
						providerName=""System.Data.SqlClient"" />
				</connectionStrings>";
			XElement xelement = XElement.Parse( xml );

			// Act
			ConnectionStringService connectionStringService = this.MockServiceLocator.Get<ConnectionStringService>();
			List<string> actual = connectionStringService.GetConnectionStringsFromFile( xelement );

			// Assert
			Assert.That( actual, Is.Not.Null );
			Assert.That( actual.Count, Is.EqualTo( 1 ) );
			Assert.That( actual[0], Is.EqualTo( expectedString ) );
		}

		[Test]
		public void FindsTwoConnectionString() {

			// Arrange
			string expectedString1 = "This is the connection string";
			string expectedString2 = "This is the other connection string";
			string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
				<configuration>
					<connectionStrings>
						<clear />
						<add name=""ConnectionString""
							connectionString=""" + expectedString1 + @"""
							providerName=""System.Data.SqlClient"" />
						<add name=""ConnectionString""
							connectionString=""" + expectedString2 + @"""
							providerName=""System.Data.SqlClient"" />
					</connectionStrings>
				</configuration>";
			XElement xelement = XElement.Parse( xml );

			// Act
			ConnectionStringService connectionStringService = this.MockServiceLocator.Get<ConnectionStringService>();
			List<string> actual = connectionStringService.GetConnectionStringsFromFile( xelement );

			// Assert
			Assert.That( actual, Is.Not.Null );
			Assert.That( actual.Count, Is.EqualTo( 2 ) );
			Assert.That( actual[0], Is.EqualTo( expectedString1 ) );
			Assert.That( actual[1], Is.EqualTo( expectedString2 ) );
		}

		[Test]
		public void NoAdd_FindsNoConnectionString() {

			// Arrange
			string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
				<configuration>
					<connectionStrings>
						<clear />
						<notAdd name=""ConnectionString""
							connectionString=""irrelevant text""
							providerName=""System.Data.SqlClient"" />
					</connectionStrings>
				</configuration>";
			XElement xelement = XElement.Parse( xml );

			// Act
			ConnectionStringService connectionStringService = this.MockServiceLocator.Get<ConnectionStringService>();
			List<string> actual = connectionStringService.GetConnectionStringsFromFile( xelement );

			// Assert
			// Not null but no content yields empty list
			Assert.That( actual, Is.Not.Null );
			Assert.That( actual.Count, Is.EqualTo( 0 ) );
		}

		[Test]
		public void NoConnectionStrings_FindsNoConnectionString() {

			// Arrange
			string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
				<configuration>
					<irrelevantNode>
						<clear />
						<add name=""ConnectionString""
							connectionString=""irrelevant text""
							providerName=""System.Data.SqlClient"" />
					</irrelevantNode>
				</configuration>";
			XElement xelement = XElement.Parse( xml );

			// Act
			ConnectionStringService connectionStringService = this.MockServiceLocator.Get<ConnectionStringService>();
			List<string> actual = connectionStringService.GetConnectionStringsFromFile( xelement );

			// Assert
			// Not null but no content yields empty list
			Assert.That( actual, Is.Not.Null );
			Assert.That( actual.Count, Is.EqualTo( 0 ) );
		}

		[Test]
		public void NoAttribute_FindsNoConnectionString() {

			// Arrange
			string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
				<configuration>
					<connectionStrings>
						<clear />
						<add name=""ConnectionString""
							notConnectionString=""irrelevant text""
							providerName=""System.Data.SqlClient"" />
					</connectionStrings>
				</configuration>";
			XElement xelement = XElement.Parse( xml );

			// Act
			ConnectionStringService connectionStringService = this.MockServiceLocator.Get<ConnectionStringService>();
			List<string> actual = connectionStringService.GetConnectionStringsFromFile( xelement );

			// Assert
			// Not null but no content yields empty list
			Assert.That( actual, Is.Not.Null );
			Assert.That( actual.Count, Is.EqualTo( 0 ) );
		}

		[Test]
		public void NullReturnsNull() {

			// Arrange
			XElement xelement = null;

			// Act
			ConnectionStringService connectionStringService = this.MockServiceLocator.Get<ConnectionStringService>();
			List<string> actual = connectionStringService.GetConnectionStringsFromFile( xelement );

			// Assert
			// Null yields null
			Assert.That( actual, Is.Null );
		}

	}
}
