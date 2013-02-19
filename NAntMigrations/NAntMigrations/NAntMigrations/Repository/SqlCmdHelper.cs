namespace NAnt.DbMigrations.Tasks.Repository {
	using System;
	using NAnt.DbMigrations.Tasks.DataAccess;

	public interface ISqlCmdHelper {
		void RunSqlCommand( string ConnectionString, string Command );
	}

	public class SqlCmdHelper : ISqlCmdHelper {
		private readonly ISqlHelper sqlHelper;

		public SqlCmdHelper( ISqlHelper SqlHelper ) {
			if ( SqlHelper == null ) {
				throw new ArgumentNullException( "SqlHelper" );
			}
			this.sqlHelper = SqlHelper;
		}

		public void RunSqlCommand( string ConnectionString, string Command ) {
			this.sqlHelper.ExecuteQueryToFunc( ConnectionString, Command, cmd => cmd.ExecuteNonQuery() );
		}

	}
}
