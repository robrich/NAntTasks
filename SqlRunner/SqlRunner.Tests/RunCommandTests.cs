namespace NAnt.SqlRunner.Tests {

	#region using
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SqlClient;
	using System.Linq;
	using System.Text;
	using NAnt.SqlRunner.Tasks;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class RunCommandTests {

		// TODO: Set this to a valid database
		private static string ConnectionString = @"Data Source=GAMMA\SQL2008;Initial Catalog=Richardson.Cms;trusted_connection=yes";

		[Test]
		public void NonQuery_Test() {
			string command = "select top 1 * from CmsCategory";
			string expected = "-1"; // No idea why this is the result, but it is
			string results = SqlHelper.RunCommand( GetConnection(), command, ExecuteType.NonQuery );
			Assert.AreEqual( expected, results );
		}

		[Test]
		public void Scalar_Test() {
			string command = "select top 1 ParentCmsCategoryId from CmsCategory where ParentCmsCategoryId = 1";
			string expected = "1";
			string results = SqlHelper.RunCommand( GetConnection(), command, ExecuteType.Scalar );
			Assert.AreEqual( expected, results );
		}

		[Test]
		public void Reader_Test() {
			string command = "select top 2 CmsCategoryId, ParentCmsCategoryId from CmsCategory where ParentCmsCategoryId = 1";
			string expected = "<NewDataSet><Table1><CmsCategoryId>2</CmsCategoryId><ParentCmsCategoryId>1</ParentCmsCategoryId></Table1><Table1><CmsCategoryId>3</CmsCategoryId><ParentCmsCategoryId>1</ParentCmsCategoryId></Table1></NewDataSet>";
			string results = SqlHelper.RunCommand( GetConnection(), command, ExecuteType.Reader );
			Assert.AreEqual( expected, results );
		}

		private static IDbConnection GetConnection() {
			IDbConnection cmd = new SqlConnection( ConnectionString );
			return cmd;
		}

	}

}
