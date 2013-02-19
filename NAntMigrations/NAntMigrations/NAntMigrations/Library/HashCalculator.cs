namespace NAnt.DbMigrations.Tasks.Library {
	using System;
	using System.Security.Cryptography;
	using System.Text;

	public interface IHashCalculator {
		string CalculateHash( string Source );
	}

	public class HashCalculator : IHashCalculator {

		// http://blogs.msdn.com/b/csharpfaq/archive/2006/10/09/how-do-i-calculate-a-md5-hash-from-a-string_3f00_.aspx
		// http://stackoverflow.com/questions/3405705/c-sharp-calculate-md5-for-opened-file
		// http://www.jonasjohn.de/snippets/csharp/calculate-md5.htm
		public string CalculateHash( string Source ) {

			if ( string.IsNullOrEmpty( Source ) ) {
				return Source; // You asked for nothing, you got it
			}

			MD5 md5 = MD5.Create();
			byte[] inputBytes = Encoding.ASCII.GetBytes( Source );

			byte[] hash = md5.ComputeHash( inputBytes );

			return BitConverter.ToString( hash ).Replace( "-", "" );
		}

	}
}
