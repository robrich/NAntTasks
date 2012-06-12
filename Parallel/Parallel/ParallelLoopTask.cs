namespace NAnt.Parallel {
	using System.Collections.Generic;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Core.Tasks;
	using Task = System.Threading.Tasks.Task;

	[TaskName( "foreach-parallel" )]
	public class ParallelLoopTask : LoopTask {

		private List<Task> tasks = new List<Task>();

		private string[] props;
		
		/* old way */
		
		protected override void ExecuteTask() {
			props = this.Property.Split( ',' );
			base.ExecuteTask();
			Task.WaitAll( tasks.ToArray() );
		}

		protected override void DoWork( params string[] propVals ) {
		
			// Start a new task on the thread pool
			tasks.Add( Task.Factory.StartNew( () => {
			
				// Set thread-specific properties
				for ( int i = 0; i < propVals.Length; i++ ) {
					string propValue = propVals[i];
					if ( i >= props.Length ) {
						throw new BuildException( "Too many items on line", Location );
					}
					switch ( TrimType ) {
						case LoopTrim.Both:
							propValue = propValue.Trim();
							break;
						case LoopTrim.Start:
							propValue = propValue.TrimStart();
							break;
						case LoopTrim.End:
							propValue = propValue.TrimEnd();
							break;
					}
					string name = PropertyThreadMangler.GetName( props[i] );
					if ( this.Properties.Contains( name ) ) {
						this.Properties[name] = propValue;
					} else {
						this.Properties.Add( name, propValue );
					}
				}

				// Run the task
				this.ExecuteChildTasks();

			} ) );
		}

		/* new way

		protected override void ExecuteTask() {
			props = this.Property.Split( ',' );
			for ( int i = 0; i < props.Length; i++ ) {
				string name = props[i];
				if ( this.Properties.Contains(name) ) {
					this.Properties.Remove(name);
				}
				this.Properties.MarkLocal(name);
			}
			base.ExecuteTask();
			Task.WaitAll( tasks.ToArray() );
		}

		protected override void DoWork( params string[] propVals ) {
			// Start a new task on the thread pool
			tasks.Add( Task.Factory.StartNew( () => {
				base.DoWork(propVals);
			} ) );
		}

		*/

	}
}