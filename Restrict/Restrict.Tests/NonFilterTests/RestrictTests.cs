namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using NAnt.Core;
	using NAnt.Core.Types;
	using NAnt.Restrict.Filters;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class RestrictTests : NonFilterBaseTest {

		[Test]
		public void InitFindsProperties() {

			FileSet fs = new FileSet();
			Restrict restrict = new Restrict( fs );
			// Init didn't throw, ASSUME we win

			// Irrelevant but not unhelpful
			Assert.AreEqual( 0, restrict.NestedFilterCount );

		}

		[Test]
		public void NestedFilitersInited() {

			// Arrange
			MockNestedFilter mock = new MockNestedFilter( true );
			List<FilterBase> filters = new List<FilterBase>() {
				mock
			};
			FileSet fs = new FileSet();

			Restrict restrict = new Restrict( filters, fs );

			// Irrelevant but not unhelpful
			Assert.AreEqual( 1, restrict.NestedFilterCount );

			// Act
			restrict.Scan();

			// Assert
			Assert.AreEqual( true, mock.NestedInitializeCalled );

		}

		[Test]
		public void ScanFiltersTrueCorrectly() {

			// Arrange
			List<FilterBase> filters = new List<FilterBase>() {
				new MockFilter( true )
			};

			string filename = "harry.jpg";
			FileSet fs = new FileSet();
			fs.BaseDirectory = new DirectoryInfo( Path.GetDirectoryName( Assembly.GetExecutingAssembly().GetName().CodeBase.Substring( 8 ) ) ); // Get past file://
			fs.FileNames.Add( filename );

			Restrict restrict = new Restrict( filters, fs );

			// Irrelevant but not unhelpful
			Assert.AreEqual( 1, restrict.NestedFilterCount );

			// Act
			restrict.Scan();

			// Assert
			Assert.IsNotNull( restrict.FileNames );
			Assert.AreEqual( 1, restrict.FileNames.Count );
			Assert.AreEqual( filename, restrict.FileNames[0] );
			Assert.IsTrue( restrict.hasScanned );

		}

		[Test]
		public void ScanFiltersFalseCorrectly() {

			// Arrange
			List<FilterBase> filters = new List<FilterBase>() {
				new MockFilter( false )
			};

			string filename = "harry.jpg";
			FileSet fs = new FileSet();
			fs.BaseDirectory = new DirectoryInfo( Path.GetDirectoryName( Assembly.GetExecutingAssembly().GetName().CodeBase.Substring( 8 ) ) ); // Get past file://
			fs.FileNames.Add( filename );

			Restrict restrict = new Restrict( filters, fs );

			// Irrelevant but not unhelpful
			Assert.AreEqual( 1, restrict.NestedFilterCount );

			// Act
			restrict.Scan();

			// Assert
			Assert.IsNotNull( restrict.FileNames );
			if ( restrict.FileNames.Count != 0 ) {
				string[] files = new string[restrict.FileNames.Count];
				restrict.FileNames.CopyTo( files, 0 );
				Assert.Fail( "It should've failed, but we have " + string.Join( ", ", files ) + " selected" );
			}
			Assert.IsTrue( restrict.hasScanned );

		}

		[Test]
		public void ScanThrowsOnNoResults() {

			// Arrange
			List<FilterBase> filters = new List<FilterBase>() {
				new MockFilter( false )
			};

			string filename = "harry.jpg";
			FileSet fs = new FileSet();
			fs.BaseDirectory = new DirectoryInfo( Path.GetDirectoryName( Assembly.GetExecutingAssembly().GetName().CodeBase.Substring( 8 ) ) ); // Get past file://
			fs.FileNames.Add( filename );

			Restrict restrict = new Restrict( filters, fs );
			restrict.FailOnEmpty = true;

			// Irrelevant but not unhelpful
			Assert.AreEqual( 1, restrict.NestedFilterCount );

			Assert.IsFalse( restrict.hasScanned );

			// Act
			try {
				restrict.Scan();

				// Assert
				Assert.Fail( "No results didn't throw" );
			} catch ( ValidationException ex ) {
				if ( !ex.Message.Contains( "No matching files" ) ) {
					throw;
				}
				// It worked
			}

			Assert.IsNotNull( restrict.FileNames );
			if ( restrict.FileNames.Count != 0 ) {
				string[] files = new string[restrict.FileNames.Count];
				restrict.FileNames.CopyTo( files, 0 );
				Assert.Fail( "It should've failed, but we have " + string.Join( ", ", files ) + " selected" );
			}
			Assert.IsTrue( restrict.hasScanned );

		}

		[Test]
		public void ScanThrowsOnNoFiltersDefined() {

			// Arrange
			string filename = "harry.jpg";
			FileSet fs = new FileSet();
			fs.BaseDirectory = new DirectoryInfo( Path.GetDirectoryName( Assembly.GetExecutingAssembly().GetName().CodeBase.Substring( 8 ) ) ); // Get past file://
			fs.FileNames.Add( filename );

			Restrict restrict = new Restrict( null, fs );
			restrict.FailOnEmpty = true;

			// Irrelevant but not unhelpful
			Assert.AreEqual( 0, restrict.NestedFilterCount );

			Assert.IsFalse( restrict.hasScanned );

			// Act
			try {
				restrict.Scan();

				// Assert
				Assert.Fail( "No filters didn't throw" );
			} catch ( BuildException ex ) {
				if ( !ex.Message.Contains( "no filters" ) ) {
					throw;
				}
				// It worked
			}
			Assert.IsFalse( restrict.hasScanned );

		}

		[Test]
		public void ScanThrowsOnBlankFilename() {

			// Arrange
			List<FilterBase> filters = new List<FilterBase>() {
				new MockFilter( false )
			};

			string filename = "";
			FileSet fs = new FileSet();
			fs.BaseDirectory = new DirectoryInfo( Path.GetDirectoryName( Assembly.GetExecutingAssembly().GetName().CodeBase.Substring( 8 ) ) ); // Get past file://
			fs.FileNames.Add( filename );

			Restrict restrict = new Restrict( filters, fs );
			restrict.FailOnEmpty = true;

			// Irrelevant but not unhelpful
			Assert.AreEqual( 1, restrict.NestedFilterCount );

			Assert.IsFalse( restrict.hasScanned );

			// Act
			try {
				restrict.Scan();

				// Assert
				Assert.Fail( "No filters didn't throw" );
			} catch ( BuildException ex ) {
				ArgumentNullException innerEx = ex.InnerException as ArgumentNullException;
				if ( innerEx == null || innerEx.ParamName != "filename" ) {
					throw;
				}
				// It worked
			}
			Assert.IsFalse( restrict.hasScanned );

		}

		[Test]
		public void BaseDirIsFilesBaseDir() {

			// Arrange
			List<FilterBase> filters = new List<FilterBase>() {
				new MockFilter( false )
			};

			string filename = "somefile";
			FileSet fs = new FileSet();
			fs.BaseDirectory = new DirectoryInfo( Path.GetDirectoryName( Assembly.GetExecutingAssembly().GetName().CodeBase.Substring( 8 ) ) ); // Get past file://
			fs.FileNames.Add( filename );

			Restrict restrict = new Restrict( filters, fs );

			// Act
			// Nothing to do, it should be set already

			// Assert
			Assert.AreEqual( restrict.BaseDirectory, fs.BaseDirectory );
			
		}

	}

}
