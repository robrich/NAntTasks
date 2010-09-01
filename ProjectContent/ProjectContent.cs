namespace NantProjectContent {

	#region using
	using System.Collections.Generic;
	using System.IO;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Core.Types;

	#endregion

	[TaskName( "projecttocontent" )]
	public class ProjectToContent : Task {

		[TaskAttribute( "proj", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public string ProjectName { get; set; }

		[TaskAttribute( "contentfilesetid", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public string ContentFilesetRefId { get; set; }

		protected override void ExecuteTask() {

			string prop = this.ContentFilesetRefId;

			FileInfo project = new FileInfo( this.ProjectName );
			if ( !project.Exists ) {
				throw new BuildException( this.ProjectName + " doesn't exist, can't get the contents of it" );
			}

			List<string> content = ProjectHelper.GetProjectContentViaXml( project );

			FileSet contentFileset = new FileSet();
			contentFileset.BaseDirectory = project.Directory;
			if ( content != null ) {
				foreach ( string file in content ) {
					contentFileset.Includes.Add( file );
				}
			}

			this.SetFileSetByRefId( prop, contentFileset );

		}

		private void SetFileSetByRefId( string RefId, FileSet FileSet ) {
			var refs = this.Project.DataTypeReferences;
			if ( refs.ContainsKey( RefId ) ) {
				refs[RefId] = FileSet;
			} else {
				refs.Add( RefId, FileSet );
			}
		}

	}

}
