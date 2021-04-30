﻿namespace MogglesClient.PublicInterface
{
    public class MogglesConfigurationKeys
    {
        internal static string DefaultTimeoutValue = "30";
        internal static int DefaultCachingTime = 3600;
        internal static int OnErrorCachingTime = 60;
        internal static string UseMessagingDefault = "false";

        internal static string FeatureTogglesCacheKey = "featureToggles";
        internal static string PreviouslyCachedFeatureTogglesCacheKey = "previouslyCachedFeatureToggles";

        
        internal static string RootSection = "Moggles";
#if NETFULL
        internal static string ApplicationName = $"{RootSection}.ApplicationName";
        internal static string Url = $"{RootSection}.Url";
        internal static string RequestTimeout = $"{RootSection}.RequestTimeout";
        internal static string CachingTime = $"{RootSection}.CachingTime";
        internal static string Environment = $"{RootSection}.Environment";
        internal static string TestingMode = $"{RootSection}.TestingMode";
        internal static string MessageBusUrl = $"{RootSection}.MessageBusUrl";
        internal static string MessageBusUser = $"{RootSection}.MessageBusUser";
        internal static string MessageBusPassword = $"{RootSection}.MessageBusPassword";
        internal static string UseMessaging = $"{RootSection}.UseMessaging";
        internal static string CustomAssembliesToIgnore = $"{RootSection}.EnvironmentDetectorCustomAssembliesToIgnore";
        internal static string InstrumentationKey = $"{RootSection}.ApplicationInsightsInstrumentationKey";
        internal static string CacheRefreshQueue = $"{RootSection}.CacheRefreshQueue";
        internal static string TokenSigningKey = $"{RootSection}.TokenSigningKey";
        internal static string NotificationWebHook = $"{RootSection}.NotificationWebHook";

#endif

#if NETCORE
        internal const string ApplicationName = "ApplicationName";
        internal const string Url = "Url";
        internal const string RequestTimeout = "RequestTimeout";
        internal const string CachingTime = "CachingTime";
        internal const string Environment = "Environment";
        internal const string TestingMode = "TestingMode";
        internal const string MessageBusUrl = "MessageBusUrl";
        internal static string MessageBusUser = "MessageBusUser";
        internal static string MessageBusPassword = "MessageBusPassword";
        internal static string UseMessaging = "UseMessaging";
        internal static string CustomAssembliesToIgnore = "EnvironmentDetectorCustomAssembliesToIgnore";
        internal static string InstrumentationKey = "ApplicationInsightsInstrumentationKey";
        internal static string CacheRefreshQueue = "CacheRefreshQueue";
        internal static string TokenSigningKey = "TokenSigningKey";
        internal static string NotificationWebHook = "NotificationWebHook";
#endif
    }
}
