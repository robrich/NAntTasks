namespace NAnt.AmazonS3.Tasks {
	using Amazon.S3;
	using Amazon.S3.Model;
	using NAnt.Core.Attributes;

	[TaskName( "s3-delete" )]
	public class AmazonDeleteTask : AmazonBaseTask {
		[TaskAttribute( "file", Required = true )]
		public string File { get; set; }

		protected override void ExecuteS3Task() {
			using ( AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client( this.AccessKey, this.SecretAccessKey ) ) {
				DeleteObjectRequest request = new DeleteObjectRequest {
					BucketName = this.BucketName,
					Key = this.File
				};
				client.DeleteObject( request );
			}
		}

	}
}
