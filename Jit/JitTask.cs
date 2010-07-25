namespace NAntHttp {

	/*
	// Fake changing host header by setting a specific proxy
	// http://social.msdn.microsoft.com/forums/en-US/netfxnetcom/thread/1b35c665-fe32-4433-8877-a62f2d400a8e/
	var request = HttpWebRequest.Create("http://www.cisco.com/") as HttpWebRequest;
	request.Proxy = new WebProxy("198.133.219.25");

	// .net 4.0 adds a native property for this:
	// http://blogs.msdn.com/ncl/archive/2009/07/20/new-ncl-features-in-net-4-0-beta-2.aspx
	var request = HttpWebRequest.Create("http://127.0.0.1/") as HttpWebRequest;
	request.Host = "contoso.com";
	*/

	#region using
	using System;
	using System.IO;
	using System.Net;
	using System.Text;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	#endregion

	[TaskName( "jit" )]
	public class JitTask : Task {

		[TaskAttribute( "url", Required = true )]
		[StringValidator( AllowEmpty = false )]
		public string Url { get; set; }

		[TaskAttribute( "host" )]
		[StringValidator( AllowEmpty = true )]
		public string Host { get; set; }

		[TaskAttribute( "method", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string Method { get; set; }

		[TaskAttribute( "content", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string Content { get; set; }

		/// <summary>
		/// Timeout in miliseconds
		/// </summary>
		[TaskAttribute( "connectiontimeout", Required = false )]
		public int ConnectionTimeout { get; set; }

		[TaskAttribute( "statuscodeproperty", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string StatusCodeProperty { get; set; }

		protected override void ExecuteTask() {

			HttpWebRequest request = WebRequest.Create( this.Url ) as HttpWebRequest;

			if ( !string.IsNullOrEmpty( Method ) ) {
				request.Method = Method;
			}
			if ( !string.IsNullOrEmpty( Host ) ) {
				request.Host = Host;
			}

			if ( ConnectionTimeout > 0 ) {
				request.Timeout = ConnectionTimeout;
			}

			if ( !request.Method.Equals( "GET", StringComparison.OrdinalIgnoreCase ) && !string.IsNullOrEmpty( this.Content ) ) {
				Stream stream = request.GetRequestStream();
				BinaryWriter writer = new BinaryWriter( stream, Encoding.ASCII );
				byte[] encodedData = Encoding.ASCII.GetBytes( this.Content );
				writer.Write( encodedData, 0, encodedData.Length );
			}

			Project.Log( Level.Info, "Executing HTTP {0} to {1}", request.Method, request.RequestUri.OriginalString );
			Project.Log( Level.Verbose, "Content Type: {0}", request.ContentType );
			Project.Log( Level.Verbose, "Connection Timeout: {0}", request.Timeout );


			string message = string.Format( "The HTTP '{0}' request to '{1}' ", Method, Url );

			HttpWebResponse response = null;
			try {
				response = request.GetResponse() as HttpWebResponse;
			} catch ( WebException ex ) {
				message += "failed with status: " + (int)ex.Status;
				message += Environment.NewLine + ex.Message;
				throw new BuildException( message, ex );
			} catch ( Exception ex ) {
				message = string.Format( "failed:{0}{1}", Environment.NewLine, ex.Message );
				throw new BuildException( message, ex );
			}

			if ( response == null ) {
				throw new BuildException( message + "returned null" );
			}

			if ( FailOnError ) {
				if ( !StatusCodes.SuccessCodes.Contains( response.StatusCode ) ) {
					throw new BuildException( message + "returned " + response.StatusCode );
				}
			}

			if ( !string.IsNullOrEmpty( StatusCodeProperty ) ) {
				Project.Properties[StatusCodeProperty] = ((int)response.StatusCode).ToString();
			}

			Project.Log( Level.Info, "Response Status Code: {0}", (int)response.StatusCode );
			Project.Log( Level.Verbose, "Content Type: {0}", response.ContentType );
		}
	}

}
