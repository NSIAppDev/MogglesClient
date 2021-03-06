﻿using System.Collections.Generic;
using System.Linq;
using MogglesClient.PublicInterface;
using MogglesClient.Logging;

namespace MogglesClient
{ 

    public class MogglesToggleService
    {
        private readonly ICache _cache;
        private readonly IMogglesFeatureToggleProvider _featureToggleProvider;
        private readonly IMogglesLoggingService _featureToggleLoggingService;
        private readonly IMogglesConfigurationManager _mogglesConfigurationManager;

        public MogglesToggleService(ICache cache, IMogglesFeatureToggleProvider featureToggleProvider, IMogglesLoggingService featureToggleLoggingService, IMogglesConfigurationManager mogglesConfigurationManager)
        {
            _cache = cache;
            _featureToggleProvider = featureToggleProvider;
            _featureToggleLoggingService = featureToggleLoggingService;
            _mogglesConfigurationManager = mogglesConfigurationManager;
        }

        public List<FeatureToggle> GetFeatureTogglesFromCache()
        {
            var featureToggles = _cache.GetFeatureTogglesFromCache(MogglesConfigurationKeys.FeatureTogglesCacheKey);

            if (FeatureTogglesExist(featureToggles))
            {
                return featureToggles;
            }

            var previouslyCachedFeatureToggles = _cache.GetFeatureTogglesFromCache(MogglesConfigurationKeys.PreviouslyCachedFeatureTogglesCacheKey);

            if (FeatureTogglesExist(previouslyCachedFeatureToggles))
            {
                return previouslyCachedFeatureToggles;
            }

            _featureToggleLoggingService.TrackException(new MogglesClientException("Feature toggles were not cached and previous values were not available!"), _mogglesConfigurationManager.GetApplicationName(), _mogglesConfigurationManager.GetEnvironment());

            return new List<FeatureToggle>();
        }

        public void CacheFeatureToggles()
        {
            try
            {
                var featureToggles = _featureToggleProvider.GetFeatureToggles();

                _cache.CacheFeatureToggles(MogglesConfigurationKeys.FeatureTogglesCacheKey, featureToggles, _mogglesConfigurationManager.GetCachingTime());
                _cache.CacheFeatureToggles(MogglesConfigurationKeys.PreviouslyCachedFeatureTogglesCacheKey, featureToggles, isExpiringCacheEntry: false);
                _cache.SubscribeToCacheExpirationEvent(CacheFeatureToggles);

                _featureToggleLoggingService.TrackEvent("Main cache and backup cache were set successfully.", _mogglesConfigurationManager.GetApplicationName(), _mogglesConfigurationManager.GetEnvironment());
            }
            catch (MogglesClientException)
            {
                _cache.CacheFeatureToggles(MogglesConfigurationKeys.FeatureTogglesCacheKey, new List<FeatureToggle>(), _mogglesConfigurationManager.GetOnErrorCachingTime());
                _cache.SubscribeToCacheExpirationEvent(CacheFeatureToggles);
            }
        }

        private bool FeatureTogglesExist(List<FeatureToggle> featureToggles)
        {
            return featureToggles != null && featureToggles.Any();
        }
    }
}
