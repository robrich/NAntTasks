namespace NAnt.Parallel {
	using NAnt.Core.Attributes;
	using NAnt.Core.Tasks;

	[TaskName( "property-thread" )]
	public class PropertyThreadTask : PropertyTask {

		private bool mangled = false;

		protected override void ExecuteTask() {
			if ( !mangled ) {
				this.PropertyName = PropertyThreadMangler.GetName( this.PropertyName );
				mangled = true;
			}
			base.ExecuteTask();
		}

	}
}