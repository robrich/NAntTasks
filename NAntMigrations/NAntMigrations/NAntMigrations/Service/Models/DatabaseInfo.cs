namespace NAnt.DbMigrations.Tasks.Service.Models {
	public class DatabaseInfo {
		/// <summary>
		/// Either config file or path to directory to locate config files
		/// </summary>
		public string ConnectionStringSource { get; set; }

		// use above or below, defaults to above if not null

		public string Server { get; set; }
		public string Database { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		/// <summary>
		/// If TrustedConnection = true, Username and Password are ignored
		/// </summary>
		public bool TrustedConnection { get; set; }
	}
}