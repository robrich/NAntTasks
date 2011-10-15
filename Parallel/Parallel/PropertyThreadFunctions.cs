namespace NAnt.Parallel {
	using System.Threading;
	using NAnt.Core;
	using NAnt.Core.Attributes;

	[FunctionSet( "thread", "Thread" )]
	public class PropertyThreadFunctions : FunctionSetBase {
		private readonly PropertyDictionary properties;

		public PropertyThreadFunctions( Project project, PropertyDictionary properties )
			: base( project, properties ) {
			this.properties = properties;
		}

		[Function( "get-thread-id" )]
		public int GetThreadId() {
			return Thread.CurrentThread.ManagedThreadId;
		}

		[Function( "get-value" )]
		public string GetThreadValue( string Property ) {
			return this.properties[PropertyThreadMangler.GetName( Property )];
		}

	}

	[FunctionSet( "t", "Thread" )]
	public class PropertyThreadFunctionsShortcut : FunctionSetBase {
		private readonly PropertyDictionary properties;

		public PropertyThreadFunctionsShortcut( Project project, PropertyDictionary properties )
			: base( project, properties ) {
			this.properties = properties;
		}

		[Function( "id" )]
		public int GetThreadId() {
			return Thread.CurrentThread.ManagedThreadId;
		}

		[Function( "val" )]
		public string GetThreadValue( string Property ) {
			return this.properties[PropertyThreadMangler.GetName( Property )];
		}

	}
}