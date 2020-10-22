using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using MogglesClient.PublicInterface;

namespace MogglesClient.Logging
{
    public class TelemetryClientService: IMogglesLoggingService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly IMogglesConfigurationManager _mogglesConfigurationManager;

        public TelemetryClientService(IMogglesConfigurationManager mogglesConfigurationManager)
        {
            _mogglesConfigurationManager = mogglesConfigurationManager;
            var instrumentationKey = _mogglesConfigurationManager.GetInstrumentationKey();
            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                _telemetryClient = new TelemetryClient(new TelemetryConfiguration(instrumentationKey));
            }
        }

        public void TrackException(Exception ex, string application, string environment)
        {
            _telemetryClient?.TrackException(ex, GetProperties(application, environment));
        }

        public void TrackException(Exception ex, string customMessage, string application, string environment)
        {
            var props = GetProperties(application, environment);
            props.Add("CustomMessage", customMessage);
            _telemetryClient?.TrackException(ex, props);
        }

        public void TrackEvent(string eventName, string application, string environment)
        {
            _telemetryClient?.TrackEvent(eventName, GetProperties(application, environment));
        }

        private Dictionary<string, string> GetProperties(string application, string environment)
        {
            return new Dictionary<string, string> { { "Application", application }, { "Environment", environment } };
        }
    }
}
