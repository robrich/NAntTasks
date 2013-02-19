namespace NAnt.DbMigrations.Tasks.Tests {
	using System;
	using Moq;
	using Ninject;
	using Ninject.MockingKernel.Moq;
	using NUnit.Framework;

	public interface IMockServiceLocator {
		T Get<T>() where T : class;
		Mock<T> GetMock<T>() where T : class;
	}

	[Ignore]
	public class MockServiceLocator : IMockServiceLocator {
		private readonly MoqMockingKernel kernel;

		public MockServiceLocator( MoqMockingKernel Kernel ) {
			if ( Kernel == null ) {
				throw new ArgumentNullException( "Kernel" );
			}
			this.kernel = Kernel;
		}

		public T Get<T>() where T : class {
			Type t = typeof( T );
			// If you're asking for a real class, it better be a real class
			Assert.That( t.IsInterface, Is.False );
			Assert.That( t.IsAbstract, Is.False );
			return this.kernel.Get<T>();
		}

		public Mock<T> GetMock<T>() where T : class {
			Type t = typeof( T );
			// If you're asking for a mock, it better be an interface
			Assert.That( t.IsInterface, Is.True );
			return this.kernel.GetMock<T>();
		}
	}
}