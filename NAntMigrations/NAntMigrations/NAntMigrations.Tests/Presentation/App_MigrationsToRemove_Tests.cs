namespace NAnt.DbMigrations.Tasks.Tests.Presentation {
	using System.Collections.Generic;
	using NAnt.DbMigrations.Tasks.Entity;
	using NAnt.DbMigrations.Tasks.Presentation;
	using NUnit.Framework;

	[TestFixture]
	public class App_MigrationsToRemove_Tests : TestBase {

		[Test]
		public void ListsAreEqual_Excluded() {

			// Arrange
			string filename = "somefilename";
			string hash = "somehash";
			// This one matches, and shouldn't show in the results
			MigrationHistory expectedMigration = new MigrationHistory {
				Filename = filename,
				FileHash = hash,
			};
			List<MigrationHistory> source = new List<MigrationHistory> { expectedMigration };
			List<MigrationFile> fileMigrations = new List<MigrationFile> {
				// This one matches
				new MigrationFile {
					Filename = filename,
					FileHash = hash
				}
			};
			List<MigrationHistory> expectedResults = new List<MigrationHistory>(); // empty

			// Act
			App app = this.MockServiceLocator.Get<App>();
			List<MigrationHistory> actualResults = app.MigrationsToRemove( source, fileMigrations );

			// Assert
			Assert.That( actualResults, Is.Not.Null );
			Assert.That( actualResults, Is.EquivalentTo( expectedResults ) );
		}

		[Test]
		public void ExtraFiles_Excluded() {

			// Arrange
			string filename = "somefilename";
			string hash = "somehash";
			// This one matches, and shouldn't show in the results
			MigrationHistory expectedMigration = new MigrationHistory {
				Filename = filename,
				FileHash = hash,
			};
			List<MigrationHistory> source = new List<MigrationHistory> { expectedMigration };
			List<MigrationFile> fileMigrations = new List<MigrationFile> {
				// This one matches
				new MigrationFile {
					Filename = filename,
					FileHash = hash
				},
				// This one doesn't match
				new MigrationFile {
					Filename = "anotherfile",
					FileHash = "anotherhash"
				}
			};
			List<MigrationHistory> expectedResults = new List<MigrationHistory>(); // empty

			// Act
			App app = this.MockServiceLocator.Get<App>();
			List<MigrationHistory> actualResults = app.MigrationsToRemove( source, fileMigrations );

			// Assert
			Assert.That( actualResults, Is.Not.Null );
			Assert.That( actualResults, Is.EquivalentTo( expectedResults ) );
		}
		
		[Test]
		public void ExtraRows_Included() {

			// Arrange
			string filename = "somefilename";
			string hash = "somehash";
			// This one matches, and shouldn't show in the results
			MigrationHistory expectedMigration1 = new MigrationHistory {
				Filename = filename,
				FileHash = hash,
			};
			// This one is extra and should show in the results
			MigrationHistory expectedMigration2 = new MigrationHistory {
				Filename = "anotherfile",
				FileHash = "anotherhash",
			};
			List<MigrationHistory> source = new List<MigrationHistory> { expectedMigration1, expectedMigration2 };
			List<MigrationFile> fileMigrations = new List<MigrationFile> {
				// This one matches
				new MigrationFile {
					Filename = filename,
					FileHash = hash
				},
			};
			List<MigrationHistory> expectedResults = new List<MigrationHistory> {expectedMigration2}; // Only the second one

			// Act
			App app = this.MockServiceLocator.Get<App>();
			List<MigrationHistory> actualResults = app.MigrationsToRemove( source, fileMigrations );

			// Assert
			Assert.That( actualResults, Is.Not.Null );
			Assert.That( actualResults, Is.EquivalentTo( expectedResults ) );
		}
	
		[Test]
		public void DifferentFilename_Included() {

			// Arrange
			string filename = "somefilename";
			string hash = "somehash";
			MigrationHistory expectedMigration = new MigrationHistory {
				Filename = filename,
				FileHash = hash,
			};
			List<MigrationHistory> source = new List<MigrationHistory> { expectedMigration };
			List<MigrationFile> fileMigrations = new List<MigrationFile> {
				// This one doesn't match by filename
				new MigrationFile {
					Filename = filename+"doesn'tmatch",
					FileHash = hash
				}
			};
			List<MigrationHistory> expectedResults = new List<MigrationHistory> {expectedMigration};

			// Act
			App app = this.MockServiceLocator.Get<App>();
			List<MigrationHistory> actualResults = app.MigrationsToRemove( source, fileMigrations );

			// Assert
			Assert.That( actualResults, Is.Not.Null );
			Assert.That( actualResults, Is.EquivalentTo( expectedResults ) );
		}

		[Test]
		public void DifferentHash_Included() {

			// Arrange
			string filename = "somefilename";
			string hash = "somehash";
			MigrationHistory expectedMigration = new MigrationHistory {
				Filename = filename,
				FileHash = hash,
			};
			List<MigrationHistory> source = new List<MigrationHistory> { expectedMigration };
			List<MigrationFile> fileMigrations = new List<MigrationFile> {
				// This one doesn't match by hash
				new MigrationFile {
					Filename = filename,
					FileHash = hash+"doesn'tmatch"
				}
			};
			List<MigrationHistory> expectedResults = new List<MigrationHistory> { expectedMigration };

			// Act
			App app = this.MockServiceLocator.Get<App>();
			List<MigrationHistory> actualResults = app.MigrationsToRemove( source, fileMigrations );

			// Assert
			Assert.That( actualResults, Is.Not.Null );
			Assert.That( actualResults, Is.EquivalentTo( expectedResults ) );
		}

		[Test]
		public void NullFileList_Included() {

			// Arrange
			string filename = "somefilename";
			string hash = "somehash";
			MigrationHistory expectedMigration = new MigrationHistory {
				Filename = filename,
				FileHash = hash,
			};
			List<MigrationHistory> source = new List<MigrationHistory> { expectedMigration };
			List<MigrationFile> fileMigrations = null;
			List<MigrationHistory> expectedResults = new List<MigrationHistory> { expectedMigration };

			// Act
			App app = this.MockServiceLocator.Get<App>();
			List<MigrationHistory> actualResults = app.MigrationsToRemove( source, fileMigrations );

			// Assert
			Assert.That( actualResults, Is.Not.Null );
			Assert.That( actualResults, Is.EquivalentTo( expectedResults ) );
		}

	}
}
