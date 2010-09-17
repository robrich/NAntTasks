namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using NAnt.Core.Attributes;
	using NAnt.Restrict.Filters;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class AddFilterMethodTests : NonFilterBaseTest {
		
		// I hate the redundancy, but I need a concrete type to get NestedFilterCount

		[Test]
		public void TestRestrict() {

			List<Type> filters = GetFilters();
			Type baseType = typeof( FilterBase );
			Type targetType = typeof( Restrict );

			MethodInfo[] methodInfos = targetType.GetMethods( BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance );
			Assert.True( methodInfos != null && methodInfos.Length > 0, "Can't find any methods" );

			Dictionary<Type, MethodInfo> methods = new Dictionary<Type, MethodInfo>();
			foreach ( MethodInfo methodInfo in methodInfos ) {
				var parameters = methodInfo.GetParameters();
				if ( parameters == null || parameters.Length != 1 ) {
					continue;
				}

				var parameter = parameters[0];
				Type parameterType = parameter.ParameterType;
				if ( !baseType.IsAssignableFrom( parameterType ) ) {
					continue;
				}

				Assert.False( methods.ContainsKey( parameter.ParameterType ), string.Format( "{0} includes two methods that take in a {1} including {2}", targetType.Name, parameter.ParameterType, methodInfo.Name ) );
				methods.Add( parameter.ParameterType, methodInfo );
			}

			foreach ( Type filterType in filters ) {

				FilterBase filter = (FilterBase)Activator.CreateInstance( filterType );
				Restrict target = (Restrict)Activator.CreateInstance( targetType );

				MethodInfo method = methods[filterType];
				Assert.IsNotNull( method, string.Format( "Can't find a method that takes a {0} on {1}", filter, targetType.Name ) );

				method.Invoke( target, new object[] { filter } );

				Assert.AreEqual( 1, target.NestedFilterCount, "Adding a " + filterType.Name + " to " + targetType.Name + " didn't result in 1 nested filter" );

			}

			Assert.AreEqual( filters.Count, methods.Count, "There are " + filters.Count + " filters, but " + methods.Count + " add methods that take in filters" );

		}

		[Test]
		public void TestNestedFilterBase() {

			List<Type> filters = GetFilters();
			Type baseType = typeof( FilterBase );
			Type targetType = typeof(FilterNestedBase);
			Type instanceType = typeof(OrFilter); // Because FilterNestedBase is abstract, need to instanciate something else

			MethodInfo[] methodInfos = targetType.GetMethods( BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance );
			Assert.True( methodInfos != null && methodInfos.Length > 0, "Can't find any methods" );

			Dictionary<Type, MethodInfo> methods = new Dictionary<Type, MethodInfo>();
			foreach ( MethodInfo methodInfo in methodInfos ) {
				var parameters = methodInfo.GetParameters();
				if ( parameters == null || parameters.Length != 1 ) {
					continue;
				}

				var parameter = parameters[0];
				Type parameterType = parameter.ParameterType;
				if ( !baseType.IsAssignableFrom( parameterType ) ) {
					continue;
				}

				Assert.False( methods.ContainsKey( parameter.ParameterType ), string.Format( "{0} includes two methods that take in a {1} including {2}", targetType.Name, parameter.ParameterType, methodInfo.Name ) );
				methods.Add( parameter.ParameterType, methodInfo );
			}

			foreach ( Type filterType in filters ) {

				FilterBase filter = (FilterBase)Activator.CreateInstance( filterType );
				FilterNestedBase target = (FilterNestedBase)Activator.CreateInstance( instanceType );

				MethodInfo method = methods[filterType];
				Assert.IsNotNull( method, string.Format( "Can't find a method that takes a {0} on {1}", filter, targetType.Name ) );

				method.Invoke( target, new object[] { filter } );

				Assert.AreEqual( 1, target.NestedFilterCount, "Adding a " + filterType.Name + " to " + targetType.Name + " didn't result in 1 nested filter" );

			}

			Assert.AreEqual( filters.Count, methods.Count, "There are " + filters.Count + " filters, but " + methods.Count + " add methods that take in filters" );

		}

	}

}
