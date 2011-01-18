namespace NAnt.SqlRunner.Tasks {

	#region using
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.EntityClient;
	using System.Data.Odbc;
	using System.Data.OleDb;
	using System.Data.SqlClient;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;
	using NAnt.Core;
	#endregion

	public enum ExecuteType {
		NonQuery,
		Scalar,
		Reader,
	}

	public static class SqlHelper {

		public static string ExecuteCommand( string ConnectionTypeName, string ConnectionString, string ConnectionStringName, string SqlCommand, ExecuteType ExecuteType ) {

			string connstr = GetConnectionString( ConnectionString, ConnectionStringName );
			
			IDbConnection conn = CreateConnection( ConnectionTypeName );
			conn.ConnectionString = connstr;

			string result = RunCommand( conn, SqlCommand, ExecuteType );
			return result;
		}

		// TODO: Retrieve type from connection string node and EF content?
		public static string GetConnectionString( string ConnectionString, string ConnectionStringName ) {

			string results = ConnectionString;

			// Is it a file?
			bool file = false;
			try {
				if ( File.Exists( ConnectionString ) ) {
					file = true;
				}
			} catch ( Exception ) {
				// Nope
				file = false;
			}

			if ( file ) {
				// Parse the file for a <connectionStrings> node
				try {
					string fileContent = File.ReadAllText( ConnectionString );
					XDocument doc = XDocument.Parse( fileContent );
					var connStrNode = doc.Descendants( "connectionStrings" ).FirstOrDefault();
					if ( connStrNode == null ) {
						throw new BuildException( "No <connectionStrings> node found in Connection String file: " + ConnectionString );
					}
					List<string> connStrs = null;
					if ( !string.IsNullOrEmpty( ConnectionStringName ) ) {
						connStrs = (
							from c in connStrNode.Descendants( "add" )
							where c.Attribute( "name" ).Value == ConnectionStringName
							select c.Attribute( "connectionString" ).Value
							).ToList();
					} else {
						connStrs = (
							from c in connStrNode.Descendants( "add" )
							select c.Attribute( "connectionString" ).Value
							).ToList();
					}
					if ( connStrs == null || connStrs.Count < 1 ) {
						throw new BuildException( "No connection strings found in the <connectionStrings> node in Connection String file " + ConnectionString );
					} else if ( connStrs.Count == 1 ) {
						results = connStrs[0];
					} else {
						// Take the first one
						results = connStrs[0];
					}
				} catch ( BuildException ) {
					throw;
				} catch ( Exception ex ) {
					throw new BuildException( "Error parsing Connection String file " + ConnectionString + ": " + ex.Message, ex );
				}
			}

			// Is it an EF connection string and we need to rip out the real connection string?
			if ( !string.IsNullOrEmpty( results ) ) {
				string providerHeader = "provider connection string=&quot;";
				int spot = results.IndexOf( providerHeader );
				if ( spot > -1 ) {
					results = results.Substring( spot + providerHeader.Length );
					spot = results.IndexOf( "&quot;" );
					if ( spot > -1 ) {
						results = results.Substring( 0, spot );
					}
				}
			}

			if ( string.IsNullOrEmpty( results ) ) {
				throw new BuildException( "Couldn't determine connection string" );
			}

			return results;
		}

		public static IDbConnection CreateConnection( string ConnectionTypeName ) {

			Type type = null;
			if ( string.IsNullOrEmpty( ConnectionTypeName ) || ConnectionTypeName.Contains( "SqlClient" ) || ConnectionTypeName.Contains( "SqlConnection" ) ) {
				type = typeof( SqlConnection );
			} else if ( ConnectionTypeName.Contains( "Entity" ) ) {
				type = typeof( EntityConnection );
			} else if ( ConnectionTypeName.Contains( "OleDb" ) ) {
				type = typeof( OleDbConnection );
			} else if ( ConnectionTypeName.Contains( "Odbc" ) ) {
				type = typeof( OdbcConnection );
			} else {
				// TODO: Why doesn't this work?
				type = Type.GetType( ConnectionTypeName, true, true );
				/*
				Assembly assy = Assembly.Load( "filename" );
				type = assy.GetType( ConnectionTypeName, true, true );
				*/
			}

			IDbConnection conn = Activator.CreateInstance( type ) as IDbConnection;
			if ( conn == null ) {
				throw new BuildException( "Error creating a " + ConnectionTypeName );
			}

			return conn;
		}

		public static string RunCommand(IDbConnection Connection, string SqlCommand, ExecuteType ExecuteType) {

			string result = null;

			IDbCommand cmd = Connection.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = SqlCommand;
			
			try {
				Connection.Open();

				switch ( ExecuteType ) {
					case ExecuteType.NonQuery:
						int rows = cmd.ExecuteNonQuery();
						result = rows.ToString();
						break;
					case ExecuteType.Scalar:
						var obj = cmd.ExecuteScalar();
						if ( obj != null && obj != DBNull.Value ) {
							result = obj.ToString();
						} else {
							// TODO: return "null"?
						}
						break;
					case ExecuteType.Reader:
						DataSet ds = new DataSet();
						IDataReader reader = cmd.ExecuteReader();
						/* TODO: How to get an IDataAdapter from cmd?
						using ( IDataAdapter da = new SqlDataAdapter( (SqlCommand)cmd ) ) {
							da.Fill( ds );
						}
						*/
						do {
							DataTable schemaTable = reader.GetSchemaTable();
							DataTable table = new DataTable();
							foreach ( DataRow dataRow in schemaTable.Rows ) {
								DataColumn col = new DataColumn();
								col.ColumnName = dataRow["ColumnName"].ToString();
								Type type = Type.GetType( dataRow["DataType"].ToString() );
								if ( (bool)dataRow["AllowDBNull"] && type.IsValueType ) {
									Type generic = typeof( Nullable<> ).MakeGenericType( new Type[] { type } );
									type = generic;
								}
								col.DataType = type;
								table.Columns.Add( col );
							}
							ds.Tables.Add( table );
							while ( reader.Read() ) {
								DataRow row = table.NewRow();
								for ( int i = 0; i < table.Columns.Count; i++ ) {
									row[i] = reader[i];
								}
								table.Rows.Add( row );
							} 
						} while ( reader.NextResult() );
						string xml = ds.GetXml();
						if ( !string.IsNullOrEmpty( xml ) ) {
							StringBuilder sb = new StringBuilder();
							foreach ( string line in xml.Split( new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries ) ) {
								sb.Append( line.Trim() );
							}
							result = sb.ToString();
						}
						break;
					default:
						throw new ArgumentOutOfRangeException( "ExecuteType" );
				}

			} catch ( Exception ex ) {
				throw new BuildException( ex.Message, ex );
			} finally {
				if ( Connection.State != ConnectionState.Closed ) {
					Connection.Close();
				}
				Connection.Dispose();
			}

			return result;
		}

	}

}
