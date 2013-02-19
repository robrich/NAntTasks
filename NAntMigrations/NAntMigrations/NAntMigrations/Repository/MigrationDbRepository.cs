namespace NAnt.DbMigrations.Tasks.Repository {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SqlClient;
	using NAnt.DbMigrations.Tasks.DataAccess;
	using NAnt.DbMigrations.Tasks.Entity;

	public interface IMigrationDbRepository {
		string TestConnection( string ConnectionString );
		void EnsureMigrationsTableExists( string ConnectionString );
		List<MigrationHistory> GetAllMigrations( string ConnectionString );
		string AddMigration( string ConnectionString, MigrationHistory Migration );
		string RemoveMigration( string ConnnectionString, MigrationHistory Migration );
	}

	public class MigrationDbRepository : IMigrationDbRepository {
		private readonly ISqlHelper sqlHelper;

		public MigrationDbRepository( ISqlHelper SqlHelper ) {
			if ( SqlHelper == null ) {
				throw new ArgumentNullException( "SqlHelper" );
			}
			this.sqlHelper = SqlHelper;
		}

		// No news is good news, else error message
		public string TestConnection( string ConnectionString ) {
			string response = null;
			try {
				string query = "SELECT TOP 1 NULL FROM sys.objects";
				this.sqlHelper.ExecuteQueryToFunc( ConnectionString, query, Map: d => d.ExecuteScalar() );
				// If it didn't throw an exception, it worked
				response = null; // No news is good news
			} catch ( Exception ex ) {
				// Doesn't matter why it didn't work, it didn't
				response = ex.Message;
			}
			return response;
		}

		// FRAGILE: If you have a table named this but it doesn't match the schema, we're already pretty screwed, just fail the next command.
		public void EnsureMigrationsTableExists( string ConnectionString ) {
			string createTable = @"
IF (OBJECT_ID('dbo.MigrationHistory') IS NULL)
BEGIN
	CREATE TABLE dbo.MigrationHistory (
		[Filename] nvarchar(255) NOT NULL,
		[FileHash] nvarchar(32) NOT NULL,
		[ExecutionDate] datetime NOT NULL,
		[Version] nvarchar(40) NOT NULL,
		[DownScript] nvarchar(MAX) NULL,
		CONSTRAINT [PK_MigrationHistory] PRIMARY KEY CLUSTERED (
			[Filename] ASC
		)
	)
END";
			string setTableAsSystemObject = @"
IF ISNULL((
	SELECT is_ms_shipped
	FROM        sys.objects o
	INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
	WHERE       o.name = 'MigrationHistory'
	AND s.name = 'dbo'
),0) = 0
BEGIN
	EXEC sys.sp_MS_marksystemobject MigrationHistory
END";

			this.sqlHelper.ExecuteQueryToFunc( ConnectionString, createTable, Map: cmd => cmd.ExecuteNonQuery() );
			this.sqlHelper.ExecuteQueryToFunc( ConnectionString, setTableAsSystemObject, Map: cmd => cmd.ExecuteNonQuery() );
		}

		public List<MigrationHistory> GetAllMigrations( string ConnectionString ) {
			string query = "SELECT Filename, FileHash, ExecutionDate, Version, DownScript FROM dbo.MigrationHistory ORDER BY ExecutionDate DESC"; // Latest first so we remove via lifo
			List<MigrationHistory> results = new List<MigrationHistory>();
			this.sqlHelper.ExecuteQueryToFunc( ConnectionString, query, Map: d => {
				IDataReader reader = d.ExecuteReader();
				while ( reader.Read() ) {
					// FRAGILE: ASSUME: Data is valid
					results.Add( new MigrationHistory {
						Filename = (string)reader[0],
						FileHash = (string)reader[1],
						ExecutionDate = (DateTime)reader[2],
						Version = (string)reader[3],
						DownScript = reader[4] == DBNull.Value ? null : (string)reader[4]
					} );
				}
			} );
			return results;
		}

		/// <returns>The SQL that was executed (roughly)</returns>
		public string AddMigration( string ConnectionString, MigrationHistory Migration ) {
			string query = "INSERT INTO dbo.MigrationHistory ( [Filename], [FileHash], [ExecutionDate], [Version], [DownScript] ) VALUES ( @Filename, @FileHash, @ExecutionDate, @Version, @DownScript )";
			List<IDataParameter> parameters = new List<IDataParameter> {
				new SqlParameter( "@Filename", SqlDbType.NVarChar, 255 ) {
					Value = Migration.Filename
				},
				new SqlParameter( "@FileHash", SqlDbType.NVarChar, 32 ) {
					Value = Migration.FileHash
				},
				new SqlParameter( "@ExecutionDate", Migration.ExecutionDate ),
				new SqlParameter( "@Version", SqlDbType.NVarChar, 40 ) {
					Value = Migration.Version
				},
				new SqlParameter( "@DownScript", SqlDbType.NVarChar ) {
					Value = Migration.DownScript
				}
			};
			this.sqlHelper.ExecuteQueryToFunc( ConnectionString, query, Parameters: parameters, Map: cmd => cmd.ExecuteNonQuery() );

			string results = query
				.Replace( "@Filename", this.sqlHelper.SqlEscapeParameter( Migration.Filename ) )
				.Replace( "@FileHash", this.sqlHelper.SqlEscapeParameter( Migration.FileHash ) )
				.Replace( "@ExecutionDate", this.sqlHelper.SqlEscapeParameter( Migration.ExecutionDate.ToString("G") ) )
				.Replace( "@Version", this.sqlHelper.SqlEscapeParameter( Migration.Version ) )
				.Replace( "@DownScript", this.sqlHelper.SqlEscapeParameter( Migration.DownScript ) );
			return results;
		}

		/// <returns>The SQL that was executed (roughly)</returns>
		public string RemoveMigration( string ConnectionString, MigrationHistory Migration ) {

			string query = "DELETE FROM dbo.MigrationHistory WHERE Filename = @Filename";

			List<IDataParameter> parameters = new List<IDataParameter> {
				new SqlParameter( "@Filename", SqlDbType.NVarChar, 255 ) {
					Value = Migration.Filename
				}
			};
			this.sqlHelper.ExecuteQueryToFunc( ConnectionString, query, Parameters: parameters, Map: cmd => cmd.ExecuteNonQuery() );

			string results = query
				.Replace( "@Filename", this.sqlHelper.SqlEscapeParameter( Migration.Filename ) );
			return results;
		}

	}
}
