namespace NAnt.DbMigrations.Tasks.Tests.Presentation {
	using System.Collections.Generic;
	using NAnt.DbMigrations.Tasks.Entity;
	using NAnt.DbMigrations.Tasks.Presentation;
	using NUnit.Framework;

	[TestFixture]
	public class App_MigrationsToAdd_Tests : TestBase {

		[Test]
		public void ListsAreEqual_Excluded() {

			// Arrange
			string filename = "somefilename";
			string hash = "somehash";
			// This one matches, and shouldn't show in the results
			MigrationFile expectedMigration = new MigrationFile {
				Filename = filename,
				FileHash = hash,
			};
			List<MigrationFile> source = new List<MigrationFile> { expectedMigration };
			List<MigrationHistory> dbMigrations = new List<MigrationHistory> {
				// This one matches
				new MigrationHistory {
					Filename = filename,
					FileHash = hash
				}
			};
			List<MigrationFile> expectedResults = new List<MigrationFile>(); // empty

			// Act
			App app = this.MockServiceLocator.Get<App>();
			List<MigrationFile> actualResults = app.MigrationsToAdd( source, dbMigrations );

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
			MigrationFile expectedMigration = new MigrationFile {
				Filename = filename,
				FileHash = hash,
			};
			List<MigrationFile> source = new List<MigrationFile> { expectedMigration };
			List<MigrationHistory> dbMigrations = new List<MigrationHistory> {
				// This one matches
				new MigrationHistory {
					Filename = filename,
					FileHash = hash
				},
				// This one doesn't match
				new MigrationHistory {
					Filename = "anotherfile",
					FileHash = "anotherhash"
				}
			};
			List<MigrationFile> expectedResults = new List<MigrationFile>(); // empty

			// Act
			App app = this.MockServiceLocator.Get<App>();
			List<MigrationFile> actualResults = app.MigrationsToAdd( source, dbMigrations );

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
			MigrationFile expectedMigration1 = new MigrationFile {
				Filename = filename,
				FileHash = hash,
			};
			// This one is extra and should show in the results
			MigrationFile expectedMigration2 = new MigrationFile {
				Filename = "anotherfile",
				FileHash = "anotherhash",
			};
			List<MigrationFile> source = new List<MigrationFile> { expectedMigration1, expectedMigration2 };
			List<MigrationHistory> dbMigrations = new List<MigrationHistory> {
				// This one matches
				new MigrationHistory {
					Filename = filename,
					FileHash = hash
				},
			};
			List<MigrationFile> expectedResults = new List<MigrationFile> {expectedMigration2}; // Only the second one

			// Act
			App app = this.MockServiceLocator.Get<App>();
			List<MigrationFile> actualResults = app.MigrationsToAdd( source, dbMigrations );

			// Assert
			Assert.That( actualResults, Is.Not.Null );
			Assert.That( actualResults, Is.EquivalentTo( expectedResults ) );
		}
	
		[Test]
		public void DifferentFilename_Included() {

			// Arrange
			string filename = "somefilename";
			string hash = "somehash";
			MigrationFile expectedMigration = new MigrationFile {
				Filename = filename,
				FileHash = hash,
			};
			List<MigrationFile> source = new List<MigrationFile> { expectedMigration };
			List<MigrationHistory> dbMigrations = new List<MigrationHistory> {
				// This one doesn't match by filename
				new MigrationHistory {
					Filename = filename+"doesn'tmatch",
					FileHash = hash
				}
			};
			List<MigrationFile> expectedResults = new List<MigrationFile> {expectedMigration};

			// Act
			App app = this.MockServiceLocator.Get<App>();
			List<MigrationFile> actualResults = app.MigrationsToAdd( source, dbMigrations );

			// Assert
			Assert.That( actualResults, Is.Not.Null );
			Assert.That( actualResults, Is.EquivalentTo( expectedResults ) );
		}

		[Test]
		public void DifferentHash_Included() {

			// Arrange
			string filename = "somefilename";
			string hash = "somehash";
			MigrationFile expectedMigration = new MigrationFile {
				Filename = filename,
				FileHash = hash,
			};
			List<MigrationFile> source = new List<MigrationFile> { expectedMigration };
			List<MigrationHistory> dbMigrations = new List<MigrationHistory> {
				// This one doesn't match by hash
				new MigrationHistory {
					Filename = filename,
					FileHash = hash+"doesn'tmatch"
				}
			};
			List<MigrationFile> expectedResults = new List<MigrationFile> { expectedMigration };

			// Act
			App app = this.MockServiceLocator.Get<App>();
			List<MigrationFile> actualResults = app.MigrationsToAdd( source, dbMigrations );

			// Assert
			Assert.That( actualResults, Is.Not.Null );
			Assert.That( actualResults, Is.EquivalentTo( expectedResults ) );
		}

		[Test]
		public void NullDbList_Included() {

			// Arrange
			string filename = "somefilename";
			string hash = "somehash";
			MigrationFile expectedMigration = new MigrationFile {
				Filename = filename,
				FileHash = hash,
			};
			List<MigrationFile> source = new List<MigrationFile> { expectedMigration };
			List<MigrationHistory> dbMigrations = null;
			List<MigrationFile> expectedResults = new List<MigrationFile> { expectedMigration };

			// Act
			App app = this.MockServiceLocator.Get<App>();
			List<MigrationFile> actualResults = app.MigrationsToAdd( source, dbMigrations );

			// Assert
			Assert.That( actualResults, Is.Not.Null );
			Assert.That( actualResults, Is.EquivalentTo( expectedResults ) );
		}

	}
}
