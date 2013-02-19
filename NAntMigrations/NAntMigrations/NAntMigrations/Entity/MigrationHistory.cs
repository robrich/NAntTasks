namespace NAnt.DbMigrations.Tasks.Entity {
	using System;

	public class MigrationHistory {
		// Primary key
		public string Filename { get; set; }
		public string FileHash { get; set; }
		public DateTime ExecutionDate { get; set; }
		public string Version { get; set; }
		public string DownScript { get; set; }
	}
}