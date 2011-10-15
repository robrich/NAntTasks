namespace NAnt.Parallel {
	using System.Threading;

	public static class PropertyThreadMangler {
		public static string GetName( string Name ) {
			return Name + "-" + Thread.CurrentThread.ManagedThreadId;
		}
	}
}