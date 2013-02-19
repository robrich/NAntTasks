namespace NAnt.DbMigrations.Tasks.Tests {
	using Moq;
	using NUnit.Framework;
	using Ninject;
	using Ninject.MockingKernel.Moq;

	[TestFixture]
	public class TestBase {

		private MoqMockingKernel kernel;
		private MockRepository mockRepository;
		protected IMockServiceLocator MockServiceLocator { get; private set; }

		[SetUp]
		public void SetupMockRepository() {
			// MockBehavior.Strict says "I have to impliment every use"
			// DefaultValue.Mock means "recursive fakes"
			NinjectSettings settings = new NinjectSettings();
			settings.SetMockBehavior( MockBehavior.Strict );
			this.kernel = new MoqMockingKernel( settings );
			this.mockRepository = this.kernel.MockRepository;
			this.mockRepository.DefaultValue = DefaultValue.Mock;
			this.MockServiceLocator = new MockServiceLocator( this.kernel );
		}

		[TearDown]
		public void TearDownMockRepository() {
			this.mockRepository.VerifyAll();
			this.mockRepository = null;
			this.MockServiceLocator = null;
			this.kernel = null;
		}

	}
}
