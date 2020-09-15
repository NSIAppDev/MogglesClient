#if NETCORE
using System;
using Microsoft.Extensions.Configuration;
using MogglesClient.PublicInterface;

namespace MogglesClient
{
    public class NetCoreMogglesConfigurationManager: IMogglesConfigurationManager
    {
        private IConfiguration Configuration { get; set; }

        public NetCoreMogglesConfigurationManager(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string GetApplicationName()
        {
            return Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.ApplicationName] ?? throw new MogglesClientException("There is no \"Application\" value defined in the configuration file");
        }

        public string GetEnvironment()
        {
            return Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.Environment] ?? throw new MogglesClientException("There is no \"Environment\" value defined in the configuration file");
        }

        public string GetMessageBusUrl()
        {
            return Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.MessageBusUrl];
        }

        public string GetMessageBusUser()
        {
            return Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.MessageBusUser];
        }

        public string GetMessageBusPassword()
        {
            return Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.MessageBusPassword];
        }

        public string GetTogglesUrl()
        {
            return Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.Url] ?? throw new MogglesClientException("There is no \"Url\" value defined in the configuration file");
        }

        public string GetTokenSigningKey()
        {
            return Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.TokenSigningKey] ?? throw new MogglesClientException("There is no \"TokenSigningKey\" value defined in the configuration file");

        }

        public TimeSpan GetTimeoutValue()
        {
            var timeoutString =
                Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.RequestTimeout] ?? MogglesConfigurationKeys.DefaultTimeoutValue;
            return TimeSpan.FromSeconds(int.Parse(timeoutString));
        }

        public DateTimeOffset GetCachingTime()
        {
            var cachingTime = Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.CachingTime];

            return cachingTime != null
                ? DateTimeOffset.UtcNow.AddSeconds(Int32.Parse(cachingTime))
                : DateTimeOffset.UtcNow.AddSeconds(MogglesConfigurationKeys.DefaultCachingTime);
        }

        public DateTimeOffset GetOnErrorCachingTime()
        {
            return DateTimeOffset.UtcNow.AddSeconds(MogglesConfigurationKeys.OnErrorCachingTime);
        }

        public bool IsApplicationInTestingMode()
        {
            var isApplicationInTestingMode =
                Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.TestingMode];

            return isApplicationInTestingMode != null && Convert.ToBoolean(isApplicationInTestingMode);
        }

        public bool GetFeatureToggleValueFromConfig(string name)
        {
            return Convert.ToBoolean(
                Configuration.GetSection(MogglesConfigurationKeys.RootSection)[name]);
        }

        public bool IsMessagingEnabled()
        {
            var useMessaging = Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.UseMessaging] ?? MogglesConfigurationKeys.UseMessagingDefault;
            return Convert.ToBoolean(useMessaging);
        }
        public string[] GetCustomAssemblies()
        {
            var customAssembliesString = Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.CustomAssembliesToIgnore];
            return !string.IsNullOrEmpty(customAssembliesString) ? customAssembliesString.Split(',') : new string[] {};
        }

        public string GetInstrumentationKey()
        {
            return Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.InstrumentationKey];
        }

        public string GetCacheRefreshQueue()
        {
            return Configuration.GetSection(MogglesConfigurationKeys.RootSection)[MogglesConfigurationKeys.CacheRefreshQueue];
        }
    }
}
#endif