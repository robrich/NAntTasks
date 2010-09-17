namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class TestForEachFilterTests : NonFilterBaseTest {

		[Test]
		public void AllFiltersHaveTests() {

			List<Type> tests = this.GetTestClasses();
			List<Type> filters = this.GetFilters();
			List<string> missingTests = new List<string>();

			foreach ( Type filter in filters ) {

				string name = filter.Name + "Tests";

				Type testType = tests.Find( t => t.Name == name );
				if ( testType == null ) {
					// Test is missing
					missingTests.Add( filter.Name );

				} else {
					// Is it testing the right type?
					FilterBaseTest test = (FilterBaseTest)Activator.CreateInstance( testType );
					Assert.AreEqual( filter.FullName, test.TestType.FullName );
				}
			}

			missingTests.Sort();

			Assert.AreEqual( 0, missingTests.Count, "Missing tests for these filters: " + string.Join( ", ", missingTests.ToArray() ) );
		}

		private List<Type> GetTestClasses() {
			List<Type> results = new List<Type>();

			Assembly assembly = Assembly.GetExecutingAssembly();
			if ( assembly == null ) {
				throw new ArgumentNullException( "Can't find NAnt.Restrict.Test.dll" );
			}

			Type baseType = typeof( FilterBaseTest );

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

			Assert.Greater( results.Count, 0, "No types found" );

			return results;
		}

	}

}
