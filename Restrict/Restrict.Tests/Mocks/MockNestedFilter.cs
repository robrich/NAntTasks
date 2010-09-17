namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using NAnt.Restrict.Filters;
	using NAnt.Restrict.Helpers;

	#endregion

	public class MockNestedFilter : FilterNestedBase {

		private bool state;
		public bool NestedInitializeCalled { get; private set; }

		public MockNestedFilter( bool State )
			: base( new List<FilterBase>() { new MockFilter( State ) } ) {
			this.state = State;
		}

		public override void NestedInitialize() {
			base.NestedInitialize();
			this.NestedInitializeCalled = true;
		}

		public override bool Filter( IFileInfo File ) {
			return state;
		}

	}

}
