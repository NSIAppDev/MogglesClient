using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MogglesClient.PublicInterface;
using MogglesContracts;

namespace MogglesClient.Messaging.EnvironmentDetector
{
    public class FeatureToggleEnvironmentDetector: IFeatureToggleEnvironmentDetector
    {
        private readonly IMogglesLoggingService _featureToggleLoggingService;
        private readonly IMogglesConfigurationManager _mogglesConfigurationManager;
        private readonly IMogglesBusService _busService;
        private readonly IAssemblyProvider _assemblyProvider;
        private readonly List<string> _assembliesToIgnore = new List<string>{"System", "Microsoft", "Autofac", "mscorlib", "EntityFramework", "Antlr3", "Antlr3.Runtime",
            "Glimpse", "Newtonsoft", "log4net", "AutoMapper", "EPPlus", "Fluent", "Kendo", "MassTransit", "MediatR", "Chutzpah", "WebGrease",
            "RabbitMQ.Client", "DotNetOpenAuth.Core", "Anonymously", "GreenPipes", "MogglesClient", "NCrontab", "NewId", "NLog", "Polly", "NSMessagingContracts", "NSAlertService", "NSSecurity", "OpsGenieAlerts"};

        public FeatureToggleEnvironmentDetector(IMogglesLoggingService featureToggleLoggingService, IMogglesConfigurationManager mogglesConfigurationManager, IMogglesBusService busService, IAssemblyProvider assemblyProvider)
        {
            _featureToggleLoggingService = featureToggleLoggingService;
            _mogglesConfigurationManager = mogglesConfigurationManager;
            _busService = busService;
            _assemblyProvider = assemblyProvider;

            AddCustomAssembliesToAssembliesToIgnoreList();
        }

        private void AddCustomAssembliesToAssembliesToIgnoreList()
        {
            var customAssemblies = _mogglesConfigurationManager.GetCustomAssemblies();
            foreach (var customAssembly in customAssemblies)
            {
                if (!_assembliesToIgnore.Contains(customAssembly))
                {
                    _assembliesToIgnore.Add(customAssembly);
                }
            }
        }

        public void RegisterDeployedToggles()
        {
            var featureToggleNames = GetDeployedFeatureToggles();

            var environment = _mogglesConfigurationManager.GetEnvironment();
            var application = _mogglesConfigurationManager.GetApplicationName();

            _busService.Publish(new RegisteredTogglesUpdate
            {
                AppName = application,
                Environment = environment,
                FeatureToggles = featureToggleNames
            });
        }

        private string[] GetDeployedFeatureToggles()
        {
            Assembly[] assemblies = GetValidAssemblies();

            List<string> featureToggleNames = new List<string>();

            foreach (var domainAssembly in assemblies)
            {
                try
                {
                    Type[] assemblyTypes = domainAssembly.GetTypes();
                    var assemblyFeatureToggleNames = (from assemblyType in assemblyTypes
                                                      where assemblyType.IsSubclassOf(typeof(MogglesFeatureToggle))
                                                      select assemblyType.Name).ToList();

                    featureToggleNames.AddRange(assemblyFeatureToggleNames);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"CurrentDomainAssembly={domainAssembly.FullName}");
                    foreach (Exception exSub in ex.LoaderExceptions)
                    {
                        sb.AppendLine(exSub.Message);
                    }

                    _featureToggleLoggingService.TrackException(ex, sb.ToString(), _mogglesConfigurationManager.GetApplicationName(), _mogglesConfigurationManager.GetEnvironment());
                }
            }

            return featureToggleNames.ToArray();
        }

        private Assembly[] GetValidAssemblies()
        {
            var assembliesNames = Assembly.GetEntryAssembly()?.GetReferencedAssemblies();

            var validAssemblies = assembliesNames?.Where(assembly => !_assembliesToIgnore.Any(assembly.FullName.Contains)).ToList();
            validAssemblies?.Add(Assembly.GetEntryAssembly()?.GetName());

            var assemblies = validAssemblies?.Select(Assembly.Load).Where(a => !a.GlobalAssemblyCache);

            return assemblies?.ToArray();
        }
    }
}
