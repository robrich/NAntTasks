namespace NAntHttp {

	#region using
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	#endregion

	public static class StatusCodes {

		public static readonly List<HttpStatusCode> SuccessCodes = new List<HttpStatusCode> {
			HttpStatusCode.OK,
			HttpStatusCode.Created,
			HttpStatusCode.Accepted,
			HttpStatusCode.NonAuthoritativeInformation,
			HttpStatusCode.NoContent,
			HttpStatusCode.ResetContent,
			HttpStatusCode.PartialContent
		};

	}

	public static class StreamHelper {

		public static string StreamToText( Stream stream, bool rewindBefore ) {
			if ( rewindBefore ) {
				stream.Position = 0;
			}
			TextReader reader = new StreamReader( stream );
			string content = reader.ReadToEnd();
			return content;
		}
		
	}

}
