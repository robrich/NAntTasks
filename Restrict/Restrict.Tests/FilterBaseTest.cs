namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using System.Collections.Generic;
	using NAnt.Restrict.Filters;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class FilterBaseTest {

		public FilterBaseTest( Type TestType ) {
			this.TestType = TestType;
		}

		public Type TestType { get; private set; }

		/// <summary>
		/// If this is the only way we create instances of a Filter, we're confident we're always testing the thing we think we are
		/// </summary>
		protected FilterBase CreateInstance( List<object> ConstructorParameters = null ) {
			FilterBase results = null;
			if ( ConstructorParameters != null && ConstructorParameters.Count > 0 ) {
				results = (FilterBase)Activator.CreateInstance( this.TestType, ConstructorParameters.ToArray() );
			} else {
				results = (FilterBase)Activator.CreateInstance( this.TestType );
			}

			Assert.IsNotNull( results );
			Assert.AreEqual( this.TestType.FullName, results.GetType().FullName );
			return results;
		}

		protected MockFileInfo existingFile {
			get {
				return new MockFileInfo( "ExistingFile" ) {
					Exists_Mock = true
				};
			}
		}
		protected MockFileInfo notFoundFile {
			get {
				return new MockFileInfo( "NotFoundFile" ) {
					Exists_Mock = false
				};
			}
		}

	}

}
