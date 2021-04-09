using MogglesClient.Messaging;
using MogglesClient.Logging;
using System.Collections.Generic;
using MogglesClient.Messaging.EnvironmentDetector;
using MogglesClient.PublicInterface.Notifications;

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
        public static Moggles ConfigureAndStartClient(IMogglesConfigurationManager configurationManager = null, IMogglesLoggingService loggingService = null)
        {
            lock (Padlock)
            {
                var instance = (Moggles)MogglesContainer.Resolve<Moggles>();
                if (instance == null)
                {
                    instance = new Moggles(configurationManager, loggingService);
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
        public static Moggles ConfigureAndStartClient(IConfiguration configuration, IMogglesLoggingService loggingService = null)
        {
            lock (Padlock)
            {
                var instance = (Moggles)MogglesContainer.Resolve<Moggles>();
                if (instance == null)
                {
                    instance = new Moggles(configuration, loggingService);
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
        private Moggles(IMogglesConfigurationManager configurationManager, IMogglesLoggingService loggingService)
        {
            RegisterComponentsForNetFull(configurationManager, loggingService);
            Init();
        }
#endif

#if NETCORE
        private Moggles(IConfiguration configuration, IMogglesLoggingService loggingService)
        {
            RegisterComponentsForNetCore(configuration, loggingService);
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
            _busService = CreateBusService();
            MogglesContainer.Register(_busService);

            _featureToggleEnvironmentDetector = new FeatureToggleEnvironmentDetector(_featureToggleLoggingService, _mogglesConfigurationManager, _busService, new AssemblyProvider());
            MogglesContainer.Register(_featureToggleEnvironmentDetector);
        }

#if NET452
        private IMogglesBusService CreateBusService() => new LegacyMogglesBusService(_mogglesConfigurationManager);
#else
        private IMogglesBusService CreateBusService() => new MogglesBusService(_mogglesConfigurationManager);
#endif

#if NETFULL
        private void RegisterComponentsForNetFull(IMogglesConfigurationManager configurationManager, IMogglesLoggingService loggingService)
        {
            _mogglesConfigurationManager = configurationManager ?? new NetFullMogglesConfigurationManager();
            MogglesContainer.Register(_mogglesConfigurationManager);

            ConfigureCommonComponents(loggingService);

            var cache = new NetFullCache();
            _featureToggleService = new MogglesToggleService(cache, _featureToggleProvider, _featureToggleLoggingService, _mogglesConfigurationManager);
            MogglesContainer.Register(_featureToggleService);
        }

#endif

#if NETCORE

        private void RegisterComponentsForNetCore(IConfiguration configuration, IMogglesLoggingService loggingService)
        {
            _mogglesConfigurationManager = new NetCoreMogglesConfigurationManager(configuration);
            MogglesContainer.Register(_mogglesConfigurationManager);

            ConfigureCommonComponents(loggingService);

            var cache = new NetCoreCache();
            _featureToggleService = new MogglesToggleService(cache, _featureToggleProvider, _featureToggleLoggingService, _mogglesConfigurationManager);
            MogglesContainer.Register(_featureToggleService);
        }
#endif

        private void ConfigureCommonComponents(IMogglesLoggingService loggingService)
        {
            _featureToggleLoggingService = loggingService ?? new TelemetryClientService(_mogglesConfigurationManager);
            MogglesContainer.Register(_featureToggleLoggingService);

            _featureToggleProvider = new MogglesServerProvider(_featureToggleLoggingService, _mogglesConfigurationManager);
            MogglesContainer.Register(_featureToggleProvider);

            var notificationService = new NotificationService(_mogglesConfigurationManager, _featureToggleLoggingService);
            MogglesContainer.Register<INotificationService>(notificationService);
        }

    }
}
