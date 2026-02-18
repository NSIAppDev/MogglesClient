using MogglesClient.PublicInterface.NotificationsCache;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MogglesClient.PublicInterface.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IMogglesConfigurationManager _mogglesConfigurationManager;
        private readonly IMogglesLoggingService _featureToggleLoggingService;
        private readonly INotificationsCache _notificationsCache;

        public NotificationService(INotificationsCache notificationsCache, IMogglesConfigurationManager mogglesConfigurationManager, IMogglesLoggingService featureToggleLoggingService)
        {
            _mogglesConfigurationManager = mogglesConfigurationManager;
            _featureToggleLoggingService = featureToggleLoggingService;
            _notificationsCache = notificationsCache;
        }

        public Task TryNotifyMissingFeatureToggle(string featureFlagName)
        {
            var message = new Message
            {
                Title = $"For Application {_mogglesConfigurationManager.GetApplicationName()} and Environment {_mogglesConfigurationManager.GetEnvironment()} the Feature Toggle with name {featureFlagName} is missing from Moggles.",
                Text = string.Empty
            };

            return SendNotification(message);
        }

        public Task TryNotifyBadAuthentication(string errorMessage)
        {
            var message = new Message
            {
                Title = $"Application {_mogglesConfigurationManager.GetApplicationName()} and Environment {_mogglesConfigurationManager.GetEnvironment()} failed to read feature flags. Error message: {errorMessage}.",
                Text = string.Empty
            };

            return SendNotification(message);
        }


        private async Task SendNotification(Message message)
        {
            try
            {
                var webHook = _mogglesConfigurationManager.GetNotificationWebHook();
                if (string.IsNullOrEmpty(webHook))
                    return;

                if (_notificationsCache.NotificationExists(message))
                    return;

                using (var client = new HttpClient { BaseAddress = new Uri(webHook) })
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    var serialized = JsonConvert.SerializeObject(message);
                    var content = new StringContent(serialized, Encoding.UTF8, "application/json");

                    await client.PostAsync(string.Empty, content);

                    var absoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(_mogglesConfigurationManager.GetNotificationCachingDuration());
                    _notificationsCache.CacheNotification(message, absoluteExpiration);
                }
            }
            catch (Exception ex)
            {
                _featureToggleLoggingService.TrackException(ex, _mogglesConfigurationManager.GetApplicationName(), _mogglesConfigurationManager.GetEnvironment());
            }
        }
    }
}

