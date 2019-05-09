using MogglesClient.Messaging;
using MogglesClient.Logging;
using System.Collections.Generic;
using MogglesClient.Messaging.EnvironmentDetector;
#if NETCORE
using Microsoft.Extensions.Configuration;
#endif

namespace MogglesClient.PublicInterface
{
    public class Moggles
    {       
        private MogglesToggleService _featureToggleService;
        private IMogglesLoggingService _featureToggleLoggingService;
        private IFeatureToggleEnvironmentDetector _featureToggleEnvironmentDetector;
        private IMogglesFeatureToggleProvider _featureToggleProvider;
        private IMogglesConfigurationManager _mogglesConfigurationManager;
        private IMogglesBusService _busService;

        private static readonly object Padlock = new object();

#if NETFULL
        public static Moggles ConfigureAndStartClient(IMogglesConfigurationManager configurationManager = null)
        {
            lock (Padlock)
            {
                var instance = (Moggles)MogglesContainer.Resolve<Moggles>();
                if (instance == null)
                {
                    instance = new Moggles(configurationManager);
                    MogglesContainer.Register(instance);
                }

                return instance;
            }
        }

        public static void ConfigureForTestingMode(IMogglesConfigurationManager configurationManager = null)
        {
            IMogglesConfigurationManager mogglesConfigurationManager = configurationManager ?? new NetFullMogglesConfigurationManager();
            MogglesContainer.Register(mogglesConfigurationManager);
        }
#endif

#if NETCORE
        public static Moggles ConfigureAndStartClient(IConfiguration configuration)
        {
            lock (Padlock)
            {
                var instance = (Moggles)MogglesContainer.Resolve<Moggles>();
                if (instance == null)
                {
                    instance = new Moggles(configuration);
                    MogglesContainer.Register(instance);
                }

                return instance;
            }
        }

        public static void ConfigureForTestingMode(IConfiguration configuration)
        {
            IMogglesConfigurationManager mogglesConfigurationManager = new NetCoreMogglesConfigurationManager(configuration);
            MogglesContainer.Register(mogglesConfigurationManager);
        }
#endif

        public List<FeatureToggle> GetAllToggles()
        {
            return _featureToggleService.GetFeatureTogglesFromCache();
        }

#if NETFULL
        private Moggles(IMogglesConfigurationManager configurationManager)
        {
            RegisterComponentsForNetFull(configurationManager);
            Init();
        }
#endif

#if NETCORE
        private Moggles(IConfiguration configuration)
        {
            RegisterComponentsForNetCore(configuration);
            Init();
        }
#endif

        private void Init()
        {
            if (_mogglesConfigurationManager.IsApplicationInTestingMode())
                return;

            _featureToggleService.CacheFeatureToggles();

            if (_mogglesConfigurationManager.IsMessagingEnabled())
            {
                ConfigureComponentsForMessaging();
                _busService.ConfigureAndStartMessageBus();
                _featureToggleEnvironmentDetector.RegisterDeployedToggles();
            }           
        }

        private void ConfigureComponentsForMessaging()
        {
            _busService = new MogglesBusService(_mogglesConfigurationManager);
            MogglesContainer.Register(_busService);

            _featureToggleEnvironmentDetector = new FeatureToggleEnvironmentDetector(_featureToggleLoggingService, _mogglesConfigurationManager, _busService);
            MogglesContainer.Register(_featureToggleEnvironmentDetector);
        }

#if NETFULL
        private void RegisterComponentsForNetFull(IMogglesConfigurationManager configurationManager)
        {
            _mogglesConfigurationManager = configurationManager ?? new NetFullMogglesConfigurationManager();
            MogglesContainer.Register(_mogglesConfigurationManager);

            ConfigureCommonComponents();

            var cache = new NetFullCache();
            _featureToggleService = new MogglesToggleService(cache, _featureToggleProvider, _featureToggleLoggingService, _mogglesConfigurationManager);
            MogglesContainer.Register(_featureToggleService);
        }

#endif

#if NETCORE

        private void RegisterComponentsForNetCore(IConfiguration configuration)
        {
            _mogglesConfigurationManager = new NetCoreMogglesConfigurationManager(configuration);
            MogglesContainer.Register(_mogglesConfigurationManager);

            ConfigureCommonComponents();

            var cache = new NetCoreCache();
            _featureToggleService = new MogglesToggleService(cache, _featureToggleProvider, _featureToggleLoggingService, _mogglesConfigurationManager);
            MogglesContainer.Register(_featureToggleService);
        }
#endif


        private void ConfigureCommonComponents()
        {
            _featureToggleLoggingService = new TelemetryClientService(_mogglesConfigurationManager);
            MogglesContainer.Register(_featureToggleLoggingService);

            _featureToggleProvider = new MogglesServerProvider(_featureToggleLoggingService, _mogglesConfigurationManager);
            MogglesContainer.Register(_featureToggleProvider);
        }

    }
}
