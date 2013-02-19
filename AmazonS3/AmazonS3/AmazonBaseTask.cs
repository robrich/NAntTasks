namespace NAnt.AmazonS3.Tasks {
	using Amazon.S3;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using System;

	public abstract class AmazonBaseTask : Task {

		[TaskAttribute( "accesskey", Required = true )]
		public string AccessKey { get; set; }

		[TaskAttribute( "secretaccesskey", Required = true )]
		public string SecretAccessKey { get; set; }

		[TaskAttribute( "bucketname", Required = true )]
		public string BucketName { get; set; }

		[TaskAttribute( "resultproperty", Required = false )]
		public string ResultProperty { get; set; }

		protected void SetResultProperty( string Result ) {
			if ( !string.IsNullOrEmpty( this.ResultProperty ) ) {
				if ( this.Properties.Contains( this.ResultProperty ) ) {
					this.Properties[this.ResultProperty] = Result;
				} else {
					this.Properties.Add( this.ResultProperty, Result );
				}
			}
		}

		protected override void ExecuteTask() {

			string result = null;
			try {
				this.ExecuteS3Task();
				result = null; // No news is good news
			} catch ( Exception ex ) {
				BuildException buildEx = ex as BuildException;
				if ( buildEx != null ) {
					this.SetResultProperty( buildEx.Message );
					throw;
				}
				AmazonS3Exception awsEx = ex as AmazonS3Exception;
				result = awsEx != null ? awsEx.ErrorCode : ex.Message;
				if ( string.IsNullOrEmpty( result ) ) {
					result = "Exception thrown: " + ex.GetType().FullName;
				}
			}

			this.SetResultProperty( result );
			if ( !string.IsNullOrEmpty( result ) && this.FailOnError ) {
				throw new BuildException( result );
			}
			if ( !string.IsNullOrEmpty( result ) ) {
				this.Log( Level.Error, result );
			}
		}

		protected abstract void ExecuteS3Task();

	}
}
