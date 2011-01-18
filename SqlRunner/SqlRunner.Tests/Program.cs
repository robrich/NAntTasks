namespace NAnt.SqlRunner.Tests {

	#region using
	using System;
	using NAnt.SqlRunner.Tasks;
	#endregion

	public class Program {

		static void Main() {

			try {
				string results = SqlHelper.ExecuteCommand( "", "web.connection.config", "IQZone.DataAccess.Properties.Settings.IQZoneDBConnection", "select top 1 DeviceId from Device", ExecuteType.Scalar );
				Console.WriteLine( results );
			} catch ( Exception ex ) {
				Console.WriteLine( "Error: " + ex.Message + ", " + ex.StackTrace );
			}
			Console.WriteLine( "Push any key to exit" );
			Console.ReadKey();

		}

	}

}