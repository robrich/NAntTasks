namespace NAnt.Restrict.Tests {

	#region using
	using NAnt.Restrict.Filters;
	using NAnt.Restrict.Helpers;

	#endregion

	public class MockFilter : FilterBase {

		private bool state;

		public MockFilter( bool State ) {
			this.state = State;
		}

		public override FilterPriority Priority {
			get { return FilterPriority.File; }
		}

		public override bool Filter( IFileInfo File ) {
			return this.state;
		}

	}

}
