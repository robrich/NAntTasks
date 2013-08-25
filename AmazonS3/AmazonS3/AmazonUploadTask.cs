namespace NAnt.AmazonS3.Tasks {
	using Amazon.S3.Transfer;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using System.IO;

	[TaskName( "s3-upload" )]
	public class AmazonUploadTask : AmazonBaseTask {
		[TaskAttribute( "sourcefile", Required = true )]
		public string SourceFile { get; set; }
		[TaskAttribute( "destinationfile", Required = true )]
		public string DestinationFile { get; set; }
		[TaskAttribute( "publicRead" )]
		public bool PublicRead { get; set; }

		protected override void ExecuteS3Task() {

			if ( !File.Exists( this.SourceFile ) ) {
				throw new BuildException( "source-file does not exist: " + this.SourceFile );
			}

			using ( TransferUtility transferUtility = new Amazon.S3.Transfer.TransferUtility( this.AccessKey, this.SecretAccessKey ) ) {
				TransferUtilityUploadRequest uploadRequest = new TransferUtilityUploadRequest {
					BucketName = this.BucketName,
					FilePath = this.SourceFile,
					Key = this.DestinationFile
				};
				if ( PublicRead ) {
					request.AddHeader( "x-amz-acl", "public-read" );
				}
				transferUtility.Upload( uploadRequest );
			}
		}

	}
}
