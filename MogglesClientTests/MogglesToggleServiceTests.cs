using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MogglesClient;
using MogglesClient.PublicInterface;
using Moq;

namespace MogglesClientTests
{
    [TestClass]
    public class MogglesToggleServiceTests
    {
        private MogglesToggleService _mogglesToggleService;
        private Mock<ICache> _cache;
        private Mock<IMogglesFeatureToggleProvider> _featureToggleProvider;
        private Mock<IMogglesLoggingService> _loggingService;
        private Mock<IMogglesConfigurationManager> _configurationManager;

        [TestInitialize]
        public void BeforeEach()
        {
            _cache = new Mock<ICache>();
            _featureToggleProvider = new Mock<IMogglesFeatureToggleProvider>();
            _loggingService = new Mock<IMogglesLoggingService>();
            _configurationManager = new Mock<IMogglesConfigurationManager>();

            _mogglesToggleService = new MogglesToggleService(_cache.Object, _featureToggleProvider.Object, _loggingService.Object, _configurationManager.Object);
        }

        [TestMethod]
        public void GetFeatureTogglesFromCache_ReturnsCachedToggles_WhenCachedTogglesExists()
        {
            //Arrange
            var expected = CreateFeatureToggles();
            _cache.Setup(x => x.GetFeatureTogglesFromCache(MogglesConfigurationKeys.FeatureTogglesCacheKey))
                .Returns(expected);

            //Act
            var result = _mogglesToggleService.GetFeatureTogglesFromCache();

            //Assert
            Assert.AreSame(expected, result);
        }

        [TestMethod]
        public void GetFeatureTogglesFromCache_ReturnsPreviouslyCachedToggles_WhenCachedTogglesDoesNotExistAndPreviouslyCachedExists()
        {
            //Arrange
            var expected = CreateFeatureToggles();
            _cache.Setup(x => x.GetFeatureTogglesFromCache(MogglesConfigurationKeys.PreviouslyCachedFeatureTogglesCacheKey))
                .Returns(expected);

            //Act
            var result = _mogglesToggleService.GetFeatureTogglesFromCache();

            //Assert
            Assert.AreSame(expected, result);
        }

        [TestMethod]
        public void GetFeatureTogglesFromCache_TracksException_WhenNoFeatureTogglesWereCached()
        {
            //Arrange

            //Act
            _mogglesToggleService.GetFeatureTogglesFromCache();

            //Assert
            _loggingService.Verify(x => x.TrackException(It.IsAny<MogglesClientException>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        [TestMethod]
        public void CacheFeatureToggles_CachesFeatureToggles()
        {
            //Arrange
            var featureToggles = CreateFeatureToggles();
            _featureToggleProvider.Setup(x => x.GetFeatureToggles()).Returns(featureToggles);

            //Act
            _mogglesToggleService.CacheFeatureToggles();

            //Assert
            _cache.Verify(x => x.CacheFeatureToggles(MogglesConfigurationKeys.FeatureTogglesCacheKey, featureToggles,
                It.IsAny<DateTimeOffset?>(), true));
            _cache.Verify(x => x.CacheFeatureToggles(MogglesConfigurationKeys.PreviouslyCachedFeatureTogglesCacheKey,
                featureToggles, It.IsAny<DateTimeOffset?>(), false));
        }

        [TestMethod]
        public void CacheFeatureToggles_CachesEmptyListOfFeatureToggles_WhenExceptionIsMet()
        {
            //Arrange
            var featureToggles = new List<FeatureToggle>();
            _featureToggleProvider.Setup(x => x.GetFeatureToggles()).Throws<MogglesClientException>();

            //Act
            _mogglesToggleService.CacheFeatureToggles();

            //Assert
            _cache.Verify(x => x.CacheFeatureToggles(MogglesConfigurationKeys.FeatureTogglesCacheKey, featureToggles,
                It.IsAny<DateTimeOffset?>(), true));
        }

        #region Test Setup
        private static List<FeatureToggle> CreateFeatureToggles()
        {
            return new List<FeatureToggle>
            {
                new FeatureToggle
                {
                    FeatureToggleName = "test",
                    IsEnabled = true
                }
            };
        }
        #endregion
    }
}
