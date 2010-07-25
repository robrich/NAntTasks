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
	using System.Net;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	#endregion

	[TaskName( "http" )]
	public class HttpTask : Task {

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

		[TaskAttribute( "contenttype", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string ContentType { get; set; }

		[TaskAttribute( "connectiontimeout", Required = false )]
		public int ConnectionTimeout { get; set; }

		[TaskAttribute( "statuscodeproperty", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string StatusCodeProperty { get; set; }

		[TaskAttribute( "responsebodyproperty", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string ResponseBodyProperty { get; set; }

		[TaskAttribute( "responseheadersproperty", Required = false )]
		[StringValidator( AllowEmpty = true )]
		public string ResponseHeadersProperty { get; set; }

		protected override void ExecuteTask() {

			string url = this.Url;
			if ( !Regex.IsMatch( this.Url, "^(f|ht)tp://" ) ) {
				url = "http://" + this.Url;
			}

			HttpWebRequest request = null;
			try {
				request = WebRequest.Create( url ) as HttpWebRequest;
			} catch ( Exception ex ) {
				throw new BuildException( string.Format( "Error creating request, probably because {0} is an invalid url", url ), ex );
			}

			if ( !string.IsNullOrEmpty( Method ) ) {
				request.Method = Method.ToUpperInvariant();
			}
			if ( !string.IsNullOrEmpty( Host ) ) {
				request.Host = Host;
			}

			if ( ConnectionTimeout > 0 ) {
				request.Timeout = ConnectionTimeout;
			}

			if ( !string.IsNullOrEmpty( ContentType ) ) {
				request.ContentType = ContentType;
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
				if ( ex.Response != null ) {
					string exContent = null;
					try {
						exContent = StreamHelper.StreamToText( ex.Response.GetResponseStream(), false );
					} catch ( Exception ) {
						exContent = null;
					}
					if ( !string.IsNullOrEmpty( exContent ) ) {
						message += Environment.NewLine + exContent;
					}
				}
				message += Environment.NewLine + ex.Message;
				throw new BuildException( message, ex );
			} catch ( Exception ex ) {
				message = string.Format( "failed:{0}{1}", Environment.NewLine, ex.Message );
				throw new BuildException( message, ex );
			}

			if ( response == null ) {
				throw new BuildException( message + "returned null" );
			}

			StringBuilder headersSB = new StringBuilder();
			foreach ( var header in response.Headers.AllKeys ) {
				headersSB.AppendLine( header + ": " + response.Headers[header] );
			}
			string headers = headersSB.ToString();

			string body = null;
			try {
				body = StreamHelper.StreamToText( response.GetResponseStream(), false );
			} catch ( Exception ) {
				body = null;
			}

			if ( FailOnError ) {
				if ( !StatusCodes.SuccessCodes.Contains( response.StatusCode ) ) {
					throw new BuildException( string.Format( "{0}returned {1}{2}{3}", message, response.StatusCode, Environment.NewLine, body ) );
				}
			}

			if ( !string.IsNullOrEmpty( StatusCodeProperty ) ) {
					Project.Properties[StatusCodeProperty] = ((int)response.StatusCode).ToString();
			}

			if ( !string.IsNullOrEmpty( ResponseHeadersProperty ) ) {
					Project.Properties[ResponseHeadersProperty] = headers;
			}

			if ( !string.IsNullOrEmpty( ResponseBodyProperty ) ) {
					Project.Properties[ResponseBodyProperty] = body;
			}

			Project.Log( Level.Info, "Response Status Code: {0}", (int)response.StatusCode );
			Project.Log( Level.Verbose, "Content Type: {0}", response.ContentType );
			Project.Log( Level.Verbose, "Results{0}Headers:{0}{1}", Environment.NewLine, headers );
			Project.Log( Level.Verbose, "Body:{0}{1}", Environment.NewLine, body );

		}

	}

}
