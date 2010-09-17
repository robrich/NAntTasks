namespace NAnt.Restrict.Filters {

	#region using
	using System;
	using System.IO;
	using System.Xml.Schema;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Restrict.Helpers;

	#endregion

	/// <summary>
	/// Selects resources by last write date.
	/// </summary>
	[ElementName( "date" )]
	public class DateFilter : FilterBase {

		private readonly DateTime epoch = new DateTime(1970,1,1);

		public DateFilter()
			: base() {
			this.DateTime = epoch;
			this.Granularity = 1; // TODO: Match platform: FAT filesystems = 2 sec; Unix = 1 sec; NTFS = 1 ms.
			this.When = TimeWhen.equal;
		}

		/// <summary>
		/// Number of miliseconds since January 1, 1970, exactly one of mills and datetime must exist
		/// </summary>
		[TaskAttribute( "millis", Required = false )]
		public double MillisecondsSinceEpoch { get; set; }

		/// <summary>
		/// The DateTime to use during comparison, exactly one of mills and datetime must exist
		/// </summary>
		[TaskAttribute( "datetime", Required = false )]
		public DateTime DateTime { get; set; }

		/// <summary>
		/// The number of milliseconds leeway to use when comparing file modification times. This is needed because not every file system supports tracking the last modified time to the millisecond level.
		/// </summary>
		[TaskAttribute( "granularity", Required = false )]
		public int Granularity { get; set; }

		/// <summary>
		/// One of "before", "after", "equal", default is "equal"
		/// </summary>
		[TaskAttribute( "when", Required = false )]
		public TimeWhen When { get; set; }

		public override FilterPriority Priority {
			get { return FilterPriority.File; }
		}

		public override bool Filter( IFileInfo File ) {

			if ( !File.Exists ) {
				return false; // Can't compare the date of a file that doesn't exist 
			}

			DateTime matchDate = this.DateTime;
			if ( MillisecondsSinceEpoch != 0 && matchDate == epoch ) {
				matchDate = matchDate.AddMilliseconds( MillisecondsSinceEpoch );
			} else if ( MillisecondsSinceEpoch != 0 ) {
				throw new ValidationException( "There must be exactly one datetime or mills, not both" );
			} else if ( this.DateTime == epoch ) {
				// You didn't set it, but do I care?
			}

			bool results = false;

			switch ( this.When ) {
				case TimeWhen.before:
					matchDate = matchDate.AddMilliseconds( Granularity );
					results = ( File.LastWriteTime.Ticks < matchDate.Ticks );
					break;
				case TimeWhen.after:
					matchDate = matchDate.AddMilliseconds( -Granularity );
					results = ( File.LastWriteTime.Ticks > matchDate.Ticks );
					break;
				case TimeWhen.equal:
					// Must match withing the window of granularity
					DateTime start = matchDate.AddMilliseconds( -Granularity );
					DateTime end = matchDate.AddMilliseconds( Granularity );
					results = ( start.Ticks <= File.LastWriteTime.Ticks ) && ( end.Ticks >= File.LastWriteTime.Ticks );
					break;
				default:
					throw new ValidationException( this.When + " is not a valid when" );
			}

			return results;
		}

	}

}
