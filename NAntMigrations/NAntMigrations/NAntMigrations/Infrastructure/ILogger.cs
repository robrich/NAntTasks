namespace NAnt.DbMigrations.Tasks.Infrastructure {
	using System;

	public interface ILogger {
		void Log( string Message, Exception ex = null );
		void Log( Exception ex );
		void Info( string Message );
	}
}
