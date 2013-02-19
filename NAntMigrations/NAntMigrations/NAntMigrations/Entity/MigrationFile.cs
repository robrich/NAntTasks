namespace NAnt.DbMigrations.Tasks.Entity {
	public class MigrationFile {
		public string Filename { get; set; }
		public string FileContent { get; set; }
		public string FileHash { get; set; }
		public string DownScript { get; set; }
	}
}
