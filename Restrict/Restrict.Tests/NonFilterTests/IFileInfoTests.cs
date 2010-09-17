namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using NAnt.Restrict.Helpers;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class IFileInfoTests : NonFilterBaseTest {

		[Test]
		public void TestIt() {

			FileInfo fi = new FileInfo( Assembly.GetExecutingAssembly().CodeBase.Substring( 8 ) ); // Get past file://
			Assert.AreEqual( true, fi.Exists );

			IFileInfo ifi = new IFileInfo( fi.FullName );

			Assert.AreEqual( fi.FullName, ifi.FullName );
			Assert.AreEqual( fi.Exists, ifi.Exists );
			Assert.AreEqual( fi.Length, ifi.Length );
			Assert.AreEqual( fi.IsReadOnly, ifi.IsReadOnly );
			Assert.AreEqual( fi.LastWriteTime, ifi.LastWriteTime );

		}

		private string contentTestFile = @"C:\Windows\System32\drivers\etc\hosts";
		
		[Test]
		public void ContentsTest() {

			FileInfo fi = new FileInfo( contentTestFile );
			Assert.AreEqual( true, fi.Exists );

			IFileInfo ifi = new IFileInfo( fi.FullName );

			Assert.AreEqual( fi.FullName, ifi.FullName );
			Assert.AreEqual( fi.Exists, ifi.Exists );
			Assert.AreEqual( fi.Length, ifi.Length );
			Assert.AreEqual( fi.IsReadOnly, ifi.IsReadOnly );
			Assert.AreEqual( fi.LastWriteTime, ifi.LastWriteTime );

			string content = ifi.Contents;
			Assert.IsNotNull( content );

			string expectedContent = File.ReadAllText( contentTestFile );
			Assert.IsNotNull( expectedContent );

			Assert.AreEqual( expectedContent, content );
			Assert.IsTrue( content.Contains( "localhost" ) );

		}


	}

}
