namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class TestAllMethodsAreTests : NonFilterBaseTest {

		[Test]
		public void TestAllClasses() {

			List<string> magicMethods = (
				from m in GetMethodsForObject( typeof( Object ) )
				select m.Name
			).ToList();

			List<Type> testFixtures = this.GetTestFixtures();

			foreach ( Type testFixture in testFixtures ) {

				List<MethodInfo> methods = this.GetMethodsForObject( testFixture );

				foreach ( MethodInfo method in methods ) {

					if ( magicMethods.Contains( method.Name ) ) {
						continue; // Methods inherited from object don't count
					}

					if ( method.IsConstructor ) {
						continue; // These are fine
					}
					if ( method.IsSpecialName ) {
						if ( method.Name.StartsWith( "get_" ) || method.Name.StartsWith( "set_" ) ) {
							continue; // Property getters and setters don't count
						}
					}

					Attribute[] typeAttributes = Attribute.GetCustomAttributes( method );

					if ( (
						from a in ( typeAttributes ?? new Attribute[0] ).ToList()
						where a is TestAttribute
						|| a is TestCaseAttribute
						select a
						).Any() ) {
						continue;
					}

					Assert.Fail( string.Format( "{0}.{1} doesn't have a Test or TestCase attribute on it", testFixture.FullName, method.Name ) );

				}

			}

		}

		private List<MethodInfo> GetMethodsForObject( Type ObjectType ) {
			List<MethodInfo> results = new List<MethodInfo>();

			MethodInfo[] methods = ObjectType.GetMethods( BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public );
			Assert.IsNotNull( methods, "No methods found on object" );
			Assert.Greater( methods.Length, 0, "No methods found on object" );

			foreach ( MethodInfo method in methods ) {
				results.Add( method );
			}

			Assert.Greater( results.Count, 0, "No methods found" );

			return results;
		}

		private List<Type> GetTestFixtures() {
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

				Attribute[] typeAttributes = Attribute.GetCustomAttributes( t );
				if ( typeAttributes == null || typeAttributes.Length == 0 ) {
					continue;
				}

				if ( !(
					from a in typeAttributes.ToList()
					where a is TestFixtureAttribute
					select a
					).Any() ) {
					Assert.Fail( t.FullName + " isn't a TestFixture" );
				}

				results.Add( t );
			}

			Assert.Greater( results.Count, 0, "No types found" );

			return results;
		}

	}

}
