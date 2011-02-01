namespace NAnt.SqlRunner.Tasks {

	#region using
	using System;
	using System.IO;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	#endregion

	[TaskName( "sql-runner" )]
	public class SqlRunner : Task {

		public SqlRunner() {
			this.ExecuteType = ExecuteType.NonQuery;
		}

		#region Properties

		// The class name to get a SqlConnection
		[BuildElement( "connection-type-name", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string ConnectionTypeName { get; set; }

		// Connection string or filename
		[TaskAttribute( "connection-string", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public string ConnectionString { get; set; }

		// If specifying a *.config file in ConnectionString and there's more than one, this is the ConnectionStringName to use
		// If none specified, the first is used
		[TaskAttribute( "connection-string-name", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string ConnectionStringName { get; set; }

		// ExecuteScalar, ExecuteReader, ExecuteNonQuery
		[TaskAttribute( "execute-type", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public ExecuteType ExecuteType { get; set; }

		// The text command(s) to run
		[TaskAttribute( "sql", Required = true )]
		public string SqlCommand { get; set; }

		// Output to a file?
		[TaskAttribute( "output-file", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string OutputFilename { get; set; }

		// Output to a variable?
		[TaskAttribute( "output-variable", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string OutputVariable { get; set; }

		#endregion

		protected override void ExecuteTask() {

			if ( !Enum.IsDefined( typeof( ExecuteType ), this.ExecuteType ) ) {
				throw new BuildException( this.ExecuteType + " isn't a valid execution-type: " + string.Join( ", ", Enum.GetNames( typeof( ExecuteType ) ) ) );
			}

			if ( string.IsNullOrEmpty( SqlCommand ) ) {
				throw new BuildException( "sql command is blank");
			}

			string results = SqlHelper.ExecuteCommand( this.ConnectionTypeName, this.ConnectionString, this.ConnectionStringName, this.SqlCommand, this.ExecuteType );

			if ( this.Verbose ) {
				this.Log( Level.Info, results );
			}

			if ( !string.IsNullOrEmpty( this.OutputFilename ) ) {
				try {
					File.WriteAllText( this.OutputFilename, results );
				} catch ( Exception ex ) {
					throw new BuildException( "Error writing output to " + this.OutputFilename + ": " + ex.Message, ex );
				}
			}

			if ( !string.IsNullOrEmpty( this.OutputVariable ) ) {
				if ( Project.Properties.Contains( this.OutputVariable ) == true ) {
					Project.Properties[this.OutputVariable] = results;
				} else {
					Project.Properties.Add( this.OutputVariable, results );
				}
			}
			
		}
	}
	
}