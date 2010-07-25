namespace NantProjectContent {

	#region using
	using System.Collections.Generic;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	#endregion

	[TaskName( "projecttocontent" )]
	public class ProjectToContent : Task {

		[TaskAttribute( "proj", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public string ProjectName { get; set; }

		[TaskAttribute( "delim", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string Delimiter { get; set; }

		[TaskAttribute( "contentproperty", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public string ContentProperty { get; set; }

		protected override void ExecuteTask() {

			string delim = this.Delimiter;
			if ( string.IsNullOrEmpty( delim ) ) {
				delim = ";";
			}
			string prop = this.ContentProperty;

			List<string> content = ProjectHelper.GetProjectContentViaXml( this.ProjectName );

			string contentString = null;
			if ( content != null ) {
				contentString = string.Join( delim, content.ToArray() );
			}

			if ( Project.Properties.Contains( prop ) == true ) {
				Project.Properties[prop] = contentString;
			} else {
				Project.Properties.Add( prop, contentString );
			}

		}

	}

}
