namespace NAnt.AmazonS3.Tasks {
	using Amazon.S3.Transfer;
	using NAnt.Core.Attributes;

	[TaskName( "s3-download" )]
	public class AmazonDownloadTask : AmazonBaseTask {
		[TaskAttribute( "sourcefile", Required = true )]
		public string SourceFile { get; set; }
		[TaskAttribute( "destinationfile", Required = true )]
		public string DestinationFile { get; set; }

		protected override void ExecuteS3Task() {
			using ( TransferUtility transferUtility = new Amazon.S3.Transfer.TransferUtility( this.AccessKey, this.SecretAccessKey ) ) {
				TransferUtilityDownloadRequest downloadRequest = new TransferUtilityDownloadRequest {
					BucketName = this.BucketName,
					Key = this.SourceFile,
					FilePath = this.DestinationFile,
				};
				// uploadRequest.AddHeader("x-amz-acl", "public-read");
				transferUtility.Download( downloadRequest );
			}
		}

	}
}
