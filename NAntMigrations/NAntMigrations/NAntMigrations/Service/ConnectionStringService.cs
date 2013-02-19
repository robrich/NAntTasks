namespace NAnt.DbMigrations.Tasks.Service {
	using System;
	using System.Collections.Generic;
	using System.Data.SqlClient;
	using System.Linq;
	using System.Xml.Linq;
	using NAnt.DbMigrations.Tasks.Repository;
	using NAnt.DbMigrations.Tasks.Service.Models;

	public interface IConnectionStringService {
		string GetConnectionString( DatabaseInfo DatabaseInfo );
		string GetFirstConnectionStringFromConfigFiles( string BasePath );
		List<string> GetConnectionStringsFromFile( XElement FileContent );
	}

	public class ConnectionStringService : IConnectionStringService {
		private readonly IConfigRepository configRepository;

		public ConnectionStringService( IConfigRepository ConfigRepository ) {
			if ( ConfigRepository == null ) {
				throw new ArgumentNullException( "ConfigRepository" );
			}
			this.configRepository = ConfigRepository;
		}

		public string GetConnectionString( DatabaseInfo DatabaseInfo ) {
			if ( !string.IsNullOrEmpty( DatabaseInfo.ConnectionStringSource ) ) {
				return this.GetFirstConnectionStringFromConfigFiles( DatabaseInfo.ConnectionStringSource );
			} else {
				return this.BuildConnectionString( DatabaseInfo );
			}
		}

		private string BuildConnectionString( DatabaseInfo DatabaseInfo ) {
			SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder();
			connectionStringBuilder.DataSource = DatabaseInfo.Server;
			connectionStringBuilder.InitialCatalog = DatabaseInfo.Database;
			if ( DatabaseInfo.TrustedConnection ) {
				connectionStringBuilder.IntegratedSecurity = true;
			} else {
				connectionStringBuilder.UserID = DatabaseInfo.Username;
				connectionStringBuilder.Password = DatabaseInfo.Password;
			}
			return connectionStringBuilder.ConnectionString;
		}

		public string GetFirstConnectionStringFromConfigFiles( string BasePath ) {
			return (
				from f in this.configRepository.GetConfigFiles( BasePath )
				where f != null
				let fc = this.configRepository.GetXmlContent( f )
				where fc != null
				from c in this.GetConnectionStringsFromFile( fc )
				where !string.IsNullOrEmpty( c )
				select c
			).FirstOrDefault();
		}

		public List<string> GetConnectionStringsFromFile( XElement FileContent ) {
			if ( FileContent == null ) {
				return null;
			}

			List<XElement> connectionStrings = null;
			if ( FileContent.Name != "connectionStrings" ) {
				connectionStrings = FileContent.Elements( "connectionStrings" ).ToList();
			} else {
				connectionStrings = new List<XElement>() {
					FileContent
				};
			}

			// FRAGILE: We're not getting the providerName, validating it's "System.Data.SqlClient", or getting the name
			return (
				from c in connectionStrings.DescendantNodes()
				let cn = c as XElement
				where cn.Name == "add"
				&& cn.HasAttributes
				let a = cn.Attributes( "connectionString" ).FirstOrDefault()
				where a != null
				&& !string.IsNullOrEmpty( a.Value )
				select a.Value
			).ToList();
		}

	}
}
