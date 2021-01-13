using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MogglesClient.Messaging;
using MogglesClient.Messaging.EnvironmentDetector;
using MogglesClient.PublicInterface;
using MogglesContracts;
using Moq;

namespace MogglesClientTests
{
    [TestClass]
    public class FeatureToggleEnvironmentDetectorTests
    {
        private IFeatureToggleEnvironmentDetector _featureToggleEnvironmentDetector;
        private Mock<IMogglesLoggingService> _loggingService;
        private Mock<IMogglesConfigurationManager> _configurationManager;
        private Mock<IMogglesBusService> _busService;
        private Mock<IAssemblyProvider> _assemblyProvider;

        private const string Application = "app";
        private const string Environment = "test";
        private readonly string[] _toggles = { "TestFeatureToggle1", "TestFeatureToggle2" };

        [TestInitialize]
        public void BeforeEach()
        {
            _loggingService = new Mock<IMogglesLoggingService>();
            _configurationManager = new Mock<IMogglesConfigurationManager>();
            _busService = new Mock<IMogglesBusService>();
            _assemblyProvider = new Mock<IAssemblyProvider>();

            _featureToggleEnvironmentDetector = new FeatureToggleEnvironmentDetector(_loggingService.Object, _configurationManager.Object, _busService.Object, _assemblyProvider.Object);
        }

        [TestMethod]
        public void RegisterDeployedToggles_PublishesToggleUpdate()
        {
            //Arrange
            _configurationManager.Setup(x => x.GetApplicationName()).Returns(Application);
            _configurationManager.Setup(x => x.GetEnvironment()).Returns(Environment);

            _assemblyProvider.Setup(x => x.GetCurrentDomainAssemblies()).Returns(new List<Assembly> {CreateAssembly("test") });

            //Act
            _featureToggleEnvironmentDetector.RegisterDeployedToggles();

            //Assert
            _busService.Verify(x => x.Publish(It.Is<RegisteredTogglesUpdate>(rtu => Verify(rtu))));
        }

        [TestMethod]
        public void RegisterDeployedToggles_DoesNothing_WhenAssemblyShouldBeIgnored()
        {
            //Arrange
            _configurationManager.Setup(x => x.GetApplicationName()).Returns(Application);
            _configurationManager.Setup(x => x.GetEnvironment()).Returns(Environment);

            _assemblyProvider.Setup(x => x.GetCurrentDomainAssemblies()).Returns(new List<Assembly> { CreateAssembly("Microsoft") });

            //Act
            _featureToggleEnvironmentDetector.RegisterDeployedToggles();

            //Assert
            _busService.Verify(x => x.Publish(It.Is<RegisteredTogglesUpdate>(rtu => Verify(rtu))), Times.Never);
        }

        #region Test Setup
        private Assembly CreateAssembly(string name)
        {
            var assemblyMock = new Mock<Assembly>();
            assemblyMock.Setup(x => x.GetTypes()).Returns(new[] { typeof(TestFeatureToggle1), typeof(TestFeatureToggle2) });
            assemblyMock.Setup(x => x.FullName).Returns(name);

            return assemblyMock.Object;
        }

        private bool Verify(RegisteredTogglesUpdate registeredTogglesUpdate)
        {
            return registeredTogglesUpdate.AppName == Application &&
                   registeredTogglesUpdate.Environment == Environment &&
                   registeredTogglesUpdate.FeatureToggles.SequenceEqual(_toggles);
        }

        private class TestFeatureToggle1 : MogglesFeatureToggle
        {
        }

        private class TestFeatureToggle2 : MogglesFeatureToggle
        {
        }
        #endregion
    }
}
