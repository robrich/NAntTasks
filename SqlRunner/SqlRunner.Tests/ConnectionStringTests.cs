namespace NAnt.SqlRunner.Tests {

	#region using
	using System;
	using NAnt.Core;
	using NAnt.SqlRunner.Tasks;
	using NUnit.Framework;
	#endregion

	[TestFixture]
	public class ConnectionStringTests {

		private static string TargetConnectionString = "Data Source=localhost;Initial Catalog=somedb;trusted_connection=yes;Integrated Security=SSPI";

		[Test]
		public void AppConfigContnet_Test() {
			string filename = "app.config";
			string expected = TargetConnectionString;
			string result = SqlHelper.GetConnectionString( filename, null );
			Assert.AreEqual( expected, result );
		}

		[Test]
		public void AppConfigContnet_NamedConnection_Test() {
			string filename = "app.config";
			string expected = TargetConnectionString.Replace( "somedb", "seconddb" );
			string result = SqlHelper.GetConnectionString( filename, "SecondInAppConfig" );
			Assert.AreEqual( expected, result );
		}

		[Test]
		public void FileToContent_Test() {
			string filename = "external.config";
			string expected = TargetConnectionString.Replace( "somedb", "externaldb" );
			string result = SqlHelper.GetConnectionString( filename, null );
			Assert.AreEqual( expected, result );
		}

		[Test]
		public void FileToContent_NamedConnection_Test() {
			string filename = "external.config";
			string expected = TargetConnectionString.Replace( "somedb", "secondexternaldb" );
			string result = SqlHelper.GetConnectionString( filename, "SecondInOtherConfig" );
			Assert.AreEqual( expected, result );
		}

		[Test]
		public void FileToContent_MissingFile_Test() {
			string filename = "missing.config";
			string result = SqlHelper.GetConnectionString( filename, null );
			Assert.AreEqual( filename, result );
		}

		[Test]
		public void FileToContent_EmptyFile_Test() {
			string filename = "external_empty.config";
			try {
				string result = SqlHelper.GetConnectionString( filename, null );
				Assert.Fail( "No connection strings in "+filename+" didn't error" );
			} catch ( BuildException ex ) {
				Assert.IsTrue( ex.Message.Contains( "No connection strings" ) );
			}
		}

		[Test]
		public void EntityFramework_Test() {
			string ef = "metadata=res://*/SomeDb.csdl|res://*/SomeDb.ssdl|res://*/SomeDb.msl;provider=System.Data.SqlClient;provider connection string=&quot;Data Source=localhost;Initial Catalog=somedb;trusted_connection=yes;Integrated Security=SSPI&quot;";
			string result = SqlHelper.GetConnectionString( ef, null );
			Assert.AreEqual( TargetConnectionString, result );
		}

		[Test]
		public void Regular_Test() {
			string result = SqlHelper.GetConnectionString( TargetConnectionString, null );
			Assert.AreEqual( TargetConnectionString, result );
		}

	}

}
