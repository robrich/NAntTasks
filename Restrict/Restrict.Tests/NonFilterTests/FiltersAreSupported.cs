namespace NAnt.Restrict.Tests {

	#region using
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using NAnt.Core.Attributes;
	using NAnt.Restrict.Filters;
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class FiltersAreSupported : NonFilterBaseTest {

		[Test]
		public void AllFiltersHaveElementNames() {

			List<Type> filters = GetFilters();
			foreach ( Type filter in filters ) {
				ElementNameAttribute element = (ElementNameAttribute)Attribute.GetCustomAttribute( filter, typeof( ElementNameAttribute ) );
				Assert.IsNotNull( element, filter.Name + " does not have an ElementName attribute" );
				Assert.True( !string.IsNullOrEmpty( element.Name ), filter.Name + " has a blank ElementName.Name" );
			}

		}

		[Test]
		public void AllFiltersHaveValidPriorities() {

			List<Type> filters = GetFilters();
			foreach ( Type filter in filters ) {

				FilterBase filterInstance = (FilterBase)Activator.CreateInstance( filter );
				Assert.IsNotNull( filterInstance );

				FilterPriority priority = filterInstance.Priority;

				Assert.IsTrue( Enum.IsDefined( typeof(FilterPriority), priority ) );

			}

		}

		[Test]
		public void AllFiltersHaveMethodInRestrict() {
			TestTypeHasAllFilterMethods( typeof( Restrict ) );
		}

		[Test]
		public void AllFiltersHaveMethodInFilterNestedBase() {
			TestTypeHasAllFilterMethods( typeof( FilterNestedBase ) );
		}

		private void TestTypeHasAllFilterMethods( Type targetType ) {

			List<Type> filters = GetFilters();
			Type baseType = typeof(FilterBase);

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

			foreach ( Type filter in filters ) {

				ElementNameAttribute filterElement = (ElementNameAttribute)Attribute.GetCustomAttribute( filter, typeof(ElementNameAttribute) );
				Assert.IsNotNull( filterElement, filter.Name + " does not have an ElementName attribute" );
				Assert.True( !string.IsNullOrEmpty( filterElement.Name ), filter.Name + " has a blank ElementName.Name" );

				Assert.True( methods.ContainsKey( filter ), string.Format( "{0} doesn't include a method that takes a {1}", targetType.Name, filter.Name ) );
				MethodInfo method = methods[filter];
				Assert.IsNotNull( method, string.Format( "Can't find a method that takes a {0} on {1}", filter, targetType.Name ) );

				string methodAndTypeName = method.Name + " on " + targetType.Name;

				BuildElementAttribute methodElement = (BuildElementAttribute)Attribute.GetCustomAttribute( method, typeof( BuildElementAttribute ) );
				Assert.IsNotNull( methodElement, string.Format( "{0} does not have an ElementName attribute", methodAndTypeName ) );
				Assert.True( !string.IsNullOrEmpty( methodElement.Name ), string.Format( "{0} has a blank ElementName.Name", methodAndTypeName ) );

				Assert.AreEqual( filterElement.Name, methodElement.Name, string.Format( "{0} has an ElementName of {1} but the filter has an ElementName of {2}", methodAndTypeName, methodElement.Name, filterElement.Name ) );

				Assert.AreEqual( "Add" + filter.Name, method.Name, string.Format( "Method is named {0} instead of Add{1}", methodAndTypeName, filter.Name ) );

			}

			Assert.AreEqual( filters.Count, methods.Count, "There are " + filters.Count + " filters, but " + methods.Count + " add methods that take in filters" );

		}

	}

}
