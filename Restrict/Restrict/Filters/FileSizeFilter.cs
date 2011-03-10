namespace NAnt.Restrict.Filters {

	#region using
	using System;
	using System.IO;
	using System.Text;
	using NAnt.Core;
	using NAnt.Core.Attributes;
	using NAnt.Restrict.Helpers;

	#endregion

	/// <summary>
	/// Selects resources by size.
	/// </summary>
	[ElementName( "filesize" )]
	public class FileSizeFilter : FilterBase {

		public FileSizeFilter()
			: base() {
			this.When = SizeWhen.equal;
		}

		/// <summary>
		/// File size (in K)
		/// </summary>
		[TaskAttribute( "size", Required = true )]
		public double SizeInK { get; set; }

		/// <summary>
		/// One of "equal", "eq", "greater", "gt", "less", "lt", "greaterorequal", "ge" (greater or equal), "notequal", "ne" (not equal), "lessorequal", "le" (less or equal)
		/// </summary>
		[TaskAttribute( "when", Required = false )]
		public SizeWhen When { get; set; }

		public override FilterPriority Priority {
			get { return FilterPriority.File; }
		}

		public override string Description() {
			StringBuilder sb = new StringBuilder();
			sb.Append( "<filesize " );
			this.AddParameter( sb, "size", this.SizeInK );
			this.AddParameter( sb, "when", this.When );
			sb.Append( "/>" );
			return sb.ToString();
		}

		public override bool Filter( IFileInfo File ) {

			if ( !File.Exists ) {
				return false; // Can't compare the date of a file that doesn't exist 
			}

			double filterSize = this.SizeInK;
			double fileSize = (double)File.Length / 1024.0;

			bool results = false;

			switch ( this.When ) {
				case SizeWhen.equal:
				case SizeWhen.eq:
					results = ( fileSize == filterSize );
					break;
				case SizeWhen.greater:
				case SizeWhen.gt:
					results = ( fileSize > filterSize );
					break;
				case SizeWhen.less:
				case SizeWhen.lt:
					results = ( fileSize < filterSize );
					break;
				case SizeWhen.greaterorequal:
				case SizeWhen.ge:
					results = ( fileSize >= filterSize );
					break;
				case SizeWhen.notequal:
				case SizeWhen.ne:
					results = ( fileSize != filterSize );
					break;
				case SizeWhen.lessorequal:
				case SizeWhen.le:
					results = ( fileSize <= filterSize );
					break;
				default:
					throw new ValidationException( this.When + " is not a valid when" );
			}

			return results;
		}

	}

}
