namespace NAntSlnToProjects {

	#region using
	using System;
	using System.Collections.Generic;
	using System.IO;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Core.Types;

	#endregion

	[TaskName( "slntoprojects" )]
	public class SlnToProjects : Task {

		[TaskAttribute( "sln", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public string SolutionName { get; set; }

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

		[TaskAttribute( "projectsfilesetid", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public string ProjectsFilesetRefId { get; set; }

		protected override void ExecuteTask() {

			string prop = this.ProjectsFilesetRefId;

			ProjectTypes projectType = ProjectTypes.Endpoint;
			if ( !string.IsNullOrEmpty( this.ProjectType ) ) {
				// If it isn't a valid enum, throw
				projectType = (ProjectTypes)Enum.Parse( typeof(ProjectTypes), this.ProjectType, true );
			}

			FileInfo sln = new FileInfo( this.SolutionName );
			if ( !sln.Exists ) {
				throw new BuildException( this.SolutionName + " doesn't exist, can't get the contents of it" );
			}

			List<string> projects = SolutionHelper.ParseSolution( sln );

			projects = SolutionHelper.FilterProjects( sln.DirectoryName, projects, this.ExistsOnly, projectType );

			FileSet projectsFileset = new FileSet();
			projectsFileset.BaseDirectory = sln.Directory;
			if ( projects != null ) {
				foreach ( string file in projects ) {
					projectsFileset.Includes.Add( file );
				}
			}

			this.SetFileSetByRefId( prop, projectsFileset );

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
