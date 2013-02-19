namespace NAnt.DbMigrations.Tasks.Service {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using NAnt.DbMigrations.Tasks.Entity;
	using NAnt.DbMigrations.Tasks.Library;
	using NAnt.DbMigrations.Tasks.Repository;

	public interface IMigrationFileService {
		List<MigrationFile> GetAllMigrations( string MigrationFilePath );
	}

	public class MigrationFileService : IMigrationFileService {
		private readonly IHashCalculator hashCalculator;
		private readonly IMigrationFileRepository migrationFileRepository;
		private readonly IMigrationFileParser migrationFileParser;

		public MigrationFileService( IHashCalculator HashCalculator, IMigrationFileRepository MigrationFileRepository, IMigrationFileParser MigrationFileParser ) {
			if ( HashCalculator == null ) {
				throw new ArgumentNullException( "HashCalculator" );
			}
			if ( MigrationFileRepository == null ) {
				throw new ArgumentNullException( "MigrationFileRepository" );
			}
			if ( MigrationFileParser == null ) {
				throw new ArgumentNullException( "MigrationFileParser" );
			}
			this.hashCalculator = HashCalculator;
			this.migrationFileRepository = MigrationFileRepository;
			migrationFileParser = MigrationFileParser;
		}

		public List<MigrationFile> GetAllMigrations( string MigrationFilePath ) {
			List<string> files = this.migrationFileRepository.GetFilenames( MigrationFilePath );
			List<MigrationFile> migrations = new List<MigrationFile>();
			foreach ( string f in ( files ?? new List<string>() ) ) {
				MigrationFile migration = new MigrationFile {
					Filename = f
				};
				migration.FileContent = this.migrationFileRepository.GetFileContent( MigrationFilePath, migration.Filename );
				migration.FileHash = this.hashCalculator.CalculateHash( migration.FileContent );
				Tuple<string, string> content = this.migrationFileParser.GetDownScript( migration.FileContent );
				migration.DownScript = content.Item1;
				migration.FileContent = content.Item2;
				migrations.Add( migration );
			}
			return (
				from f in migrations
				orderby f.Filename
				select f
			).ToList();
		}

	}
}
