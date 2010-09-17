namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using NAnt.Restrict.Helpers;

	#endregion

	public class MockFileInfo : IFileInfo {

		public MockFileInfo( string FullName )
			: base( ( FullName ?? "" ) + "IrrelevantButNotBlank" ) {
			// "IrrelevantButNotBlank" so it won't die if it's blank
			this.FullName_Mock = FullName;
			this.Exists_Mock = true;
			this.Length_Mock = 10000;
			this.IsReadOnly_Mock = false;
			this.LastWriteTime_Mock = new DateTime( 2000, 1, 1 );
			this.Contents_Mock = "";
		}

		public override string FullName { get { return this.FullName_Mock; } }
		public override bool Exists { get { return this.Exists_Mock; } }
		public override long Length { get { return this.Length_Mock; } }
		public override bool IsReadOnly { get { return this.IsReadOnly_Mock; } }
		public override DateTime LastWriteTime { get { return this.LastWriteTime_Mock; } }
		public override string Contents { get { return this.Contents_Mock; } }

		public string FullName_Mock { get; set; }
		public bool Exists_Mock { get; set; }
		public long Length_Mock { get; set; }
		public bool IsReadOnly_Mock { get; set; }
		public DateTime LastWriteTime_Mock { get; set; }
		public string Contents_Mock { get; set; }

	}

}
