namespace NAnt.Parallel {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Core.Tasks;
	using Task = System.Threading.Tasks.Task;

	[TaskName( "parallel" )]
	public class ParallelTask : NAnt.Core.Task {

		private List<Task> tasks = new List<Task>();

		[BuildElementArray( "do" )]
		public TaskContainer[] StuffToDo { get; set; }

		protected override void ExecuteTask() {
			if ( this.StuffToDo == null || this.StuffToDo.Length <= 0 ) {
				return;
			}

			for ( int index = 0; index < this.StuffToDo.Length; index++ ) {
				TaskContainer todo = this.StuffToDo[index];
				Task task = Task.Factory.StartNew( todo.Execute );
				this.tasks.Add( task );
			}

			Task.WaitAll( this.tasks.ToArray() );

		}

	}
}