namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using NAnt.Restrict.Filters;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class NonFilterBaseTest {

		/// <summary>
		/// For those tests that need to loop through all filters, here's the list of filters
		/// </summary>
		protected List<Type> GetFilters() {
			List<Type> results = new List<Type>();

			Type baseType = typeof( FilterBase );

			Assembly assembly = baseType.Assembly;
			if ( assembly == null ) {
				throw new ArgumentNullException( "Can't find NAnt.Restrict.dll" );
			}

			Type[] types = assembly.GetExportedTypes();
			foreach ( Type t in types ) {

				if ( !t.IsClass || t.IsNotPublic ) {
					continue;
				}
				if ( t.IsAbstract ) {
					continue;
				}

				if ( baseType.IsAssignableFrom( t ) ) {
					results.Add( t );
				}

			}

			Assert.Greater( results.Count, 0, "No filters found" );

			return results;
		}

	}

}
