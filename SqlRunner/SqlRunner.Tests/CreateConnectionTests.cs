namespace NAnt.SqlRunner.Tests {

	#region using
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Text;
	using NAnt.SqlRunner.Tasks;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class CreateConnectionTests {

		[Test]
		public void BlankType_Test() {
			IDbConnection connection = SqlHelper.CreateConnection( null );
			Assert.IsNotNull( connection );
			Assert.AreEqual( "System.Data.SqlClient.SqlConnection", connection.GetType().FullName );
		}

		[TestCase( "System.Data.SqlClient.SqlConnection" )]
		[TestCase( "System.Data.EntityClient.EntityConnection" )]
		[TestCase( "System.Data.OleDb.OleDbConnection" )]
		[TestCase( "System.Data.Odbc.OdbcConnection" )]
		public void TypeName_Test( string TypeName ) {
			IDbConnection connection = SqlHelper.CreateConnection( TypeName );
			Assert.IsNotNull( connection );
			Assert.AreEqual( TypeName, connection.GetType().FullName );
		}

		[TestCase( "SqlClient", "System.Data.SqlClient.SqlConnection" )]
		[TestCase( "EntityClient", "System.Data.EntityClient.EntityConnection" )]
		[TestCase( "SqlConnection", "System.Data.SqlClient.SqlConnection" )]
		[TestCase( "EntityConnection", "System.Data.EntityClient.EntityConnection" )]
		[TestCase( "OleDb", "System.Data.OleDb.OleDbConnection" )]
		[TestCase( "Odbc", "System.Data.Odbc.OdbcConnection" )]
		public void PartialTypeName_Test( string PartialTypeName, string TypeName ) {
			IDbConnection connection = SqlHelper.CreateConnection( PartialTypeName );
			Assert.IsNotNull( connection );
			Assert.AreEqual( TypeName, connection.GetType().FullName );
		}

	}

}
