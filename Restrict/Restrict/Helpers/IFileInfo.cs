namespace NAnt.Restrict.Helpers {

	#region using
	using System;
	using System.IO;

	#endregion

	public class IFileInfo {

		private string fullname = null;
		private FileInfo fileInfo = null;

		public IFileInfo( string FullName ) {
			this.fullname = FullName;
			this.fileInfo = new FileInfo( FullName );
		}

		public virtual string FullName {
			get { return this.fileInfo.FullName; }
		}
		public virtual bool Exists {
			get { return this.fileInfo.Exists; }
		}
		/// <summary>
		/// Length in bytes
		/// </summary>
		public virtual long Length {
			get { return this.fileInfo.Length; }
		}
		public virtual bool IsReadOnly {
			get { return this.fileInfo.IsReadOnly; }
		}
		public virtual DateTime LastWriteTime {
			get { return this.fileInfo.LastWriteTime; }
		}
		public virtual string Contents {
			get { return File.ReadAllText( this.fileInfo.FullName ); }
		}

	}

}
