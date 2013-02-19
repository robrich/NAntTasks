namespace NAnt.DbMigrations.Tasks.DataAccess {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SqlClient;

	public interface ISqlHelper {
		void ExecuteQueryToFunc( string ConnectionString, string Query, Action<IDbCommand> Map, List<IDataParameter> Parameters = null, CommandType CommandType = CommandType.Text );
		string SqlEscapeParameter( string Source );
	}

	public class SqlHelper : ISqlHelper {

		public void ExecuteQueryToFunc( string ConnectionString, string Query, Action<IDbCommand> Map, List<IDataParameter> Parameters = null, CommandType CommandType = CommandType.Text ) {

			if ( Map == null ) {
				throw new ArgumentNullException( "Map" );
			}
			if ( Query == null ) {
				throw new ArgumentNullException( "Query" );
			}
			if ( ConnectionString == null ) {
				throw new ArgumentNullException( "ConnectionString" );
			}

			using ( SqlConnection conn = new SqlConnection( ConnectionString ) ) {
				using ( IDbCommand cmd = conn.CreateCommand() ) {
					cmd.CommandText = Query;
					cmd.CommandType = CommandType;
					if ( Parameters != null ) {
						foreach ( IDataParameter p in Parameters ) {
							if ( p.Value == null ) {
								p.Value = DBNull.Value;
							}
							cmd.Parameters.Add( p );
						}
					}
					bool open = cmd.Connection.State != ConnectionState.Closed;

					try {
						if ( !open ) {
							cmd.Connection.Open();
						}
						Map( cmd );
					} finally {
						cmd.Parameters.Clear(); // Until the GC runs, the old connection owns the passed in parameters and the retry won't work correctly
						if ( !open && cmd.Connection.State != ConnectionState.Closed ) {
							cmd.Connection.Close();
						}
					}
				}
			}

			if ( Parameters != null ) {
				foreach ( IDataParameter p in Parameters ) {
					if ( p.Value == DBNull.Value ) {
						p.Value = null;
					}
				}
			}

		}

		public string SqlEscapeParameter( string Source ) {
			string results = null;
			if ( string.IsNullOrEmpty( Source ) ) {
				results = Source;
			} else {
				results = "'" + Source.Replace( "'", "''" ).Replace( "\\", "\\\\" ).Replace( "%", "\\%" ).Replace( "_", "\\_" ) + "'";
			}
			return results;
		}

	}
}
