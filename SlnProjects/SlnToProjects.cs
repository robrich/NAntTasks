namespace NAntSlnToProjects {

	#region using
	using System;
	using System.Collections.Generic;
	using System.IO;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	#endregion

	[TaskName( "slntoprojects" )]
	public class SlnToProjects : Task {

		[TaskAttribute( "sln", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public string Solution { get; set; }

		[TaskAttribute( "delim", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string Delimiter { get; set; }

		/// <summary>
		/// Only return "endpoint" projects? e.g. has Web.config or builds to .exe or contains reference to NUnit
		/// </summary>
		[TaskAttribute( "type", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string ProjectType { get; set; }
		// "all", "endpoint", "web", "app", "test"

		[TaskAttribute( "existsonly", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public bool ExistsOnly { get; set; }

		[TaskAttribute( "projectsproperty", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public string ProjectsProperty { get; set; }

		protected override void ExecuteTask() {

			string delim = this.Delimiter;
			if ( string.IsNullOrEmpty( delim ) ) {
				delim = ";";
			}
			string prop = this.ProjectsProperty;
			ProjectTypes projectType = ProjectTypes.Endpoint;
			if ( !string.IsNullOrEmpty( this.ProjectType ) ) {
				// If it isn't a valid enum, throw
				projectType = (ProjectTypes)Enum.Parse( typeof(ProjectTypes), this.ProjectType, true );
			}

			List<string> projects = SolutionHelper.ParseSolution( this.Solution );

			projects = SolutionHelper.FilterProjects( projects, this.ExistsOnly, projectType );

			string projectString = null;
			if ( projects != null ) {
				projectString = string.Join( delim, projects.ToArray() );
			}

			if ( Project.Properties.Contains( prop ) == true ) {
				Project.Properties[prop] = projectString;
			} else {
				Project.Properties.Add( prop, projectString );
			}

		}

	}

}
