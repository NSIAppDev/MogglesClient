#if NETFULL
using MogglesClient.PublicInterface;
using System;
using System.Configuration;

namespace MogglesClient
{
    public class NetFullMogglesConfigurationManager: IMogglesConfigurationManager
    {
        public string GetApplicationName()
        {
            return ConfigurationManager.AppSettings[MogglesConfigurationKeys.ApplicationName]
                   ?? throw new MogglesClientException(
                       "There is no \"Application\" value defined in the configuration file");
        }

        public string GetEnvironment()
        {
            return ConfigurationManager.AppSettings[MogglesConfigurationKeys.Environment]
                   ?? throw new MogglesClientException(
                       "There is no \"Environment\" value defined in the configuration file");
        }

        public string GetMessageBusUrl()
        {
            return ConfigurationManager.AppSettings[MogglesConfigurationKeys.MessageBusUrl];
        }

        public string GetMessageBusUser()
        {
            return ConfigurationManager.AppSettings[MogglesConfigurationKeys.MessageBusUser];
        }

        public string GetMessageBusPassword()
        {
            return ConfigurationManager.AppSettings[MogglesConfigurationKeys.MessageBusPassword];
        }

        public string GetTogglesUrl()
        {
            return ConfigurationManager.AppSettings[MogglesConfigurationKeys.Url] ?? throw new MogglesClientException("There is no \"Url\" value defined in the configuration file");
        }

        public string GetTokenSigningKey()
        {
            return ConfigurationManager.AppSettings[MogglesConfigurationKeys.TokenSigningKey];
        }

        public TimeSpan GetTimeoutValue()
        {
            var timeoutString = ConfigurationManager.AppSettings[MogglesConfigurationKeys.RequestTimeout] ??
                                MogglesConfigurationKeys.DefaultTimeoutValue;

            return TimeSpan.FromSeconds(int.Parse(timeoutString));
        }

        public DateTimeOffset GetCachingTime()
        {
            var cachingTime = ConfigurationManager.AppSettings[MogglesConfigurationKeys.CachingTime];
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
            var isApplicationInTestingMode = ConfigurationManager.AppSettings[MogglesConfigurationKeys.TestingMode];

            return isApplicationInTestingMode != null && Convert.ToBoolean(isApplicationInTestingMode);
        }

        public bool GetFeatureToggleValueFromConfig(string name)
        {
            return Convert.ToBoolean(ConfigurationManager.AppSettings[$"{MogglesConfigurationKeys.RootSection}.{name}"]);
        }

        public bool IsMessagingEnabled()
        {
            var useMessaging = ConfigurationManager.AppSettings[MogglesConfigurationKeys.UseMessaging] ?? MogglesConfigurationKeys.UseMessagingDefault;
            return Convert.ToBoolean(useMessaging);
        }

        public string[] GetCustomAssemblies()
        {
            var customAssembliesString = ConfigurationManager.AppSettings[MogglesConfigurationKeys.CustomAssembliesToIgnore];
            return !string.IsNullOrEmpty(customAssembliesString) ? customAssembliesString.Split(',') : new string[]{};
        }

        public string GetInstrumentationKey()
        {
            return ConfigurationManager.AppSettings[MogglesConfigurationKeys.InstrumentationKey];
        }

        public string GetCacheRefreshQueue()
        {
            return ConfigurationManager.AppSettings[MogglesConfigurationKeys.CacheRefreshQueue];
        }
    }
}
#endif