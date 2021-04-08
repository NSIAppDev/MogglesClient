using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace MogglesClient.PublicInterface.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IMogglesConfigurationManager _mogglesConfigurationManager;

        public NotificationService(IMogglesConfigurationManager mogglesConfigurationManager)
        {
            _mogglesConfigurationManager = mogglesConfigurationManager;
        }

        public void TryNotifyMissingFeatureToggle(string featureFlagName)
        {
            var webHook = _mogglesConfigurationManager.GetNotificationWebHook();

            if (string.IsNullOrEmpty(webHook))
                return;

            using (var client = new HttpClient {BaseAddress = new Uri(webHook)})
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var application = _mogglesConfigurationManager.GetApplicationName();
                var environment = _mogglesConfigurationManager.GetEnvironment();

                var message = new Message($"For Application {application} and Environment {environment} the Feature Toggle with name {featureFlagName} is missing from Moggles.");

                var serialized = JsonConvert.SerializeObject(message);

                client.PostAsync(string.Empty, new StringContent(serialized)).GetAwaiter().GetResult();
            }
        }
    }
}
