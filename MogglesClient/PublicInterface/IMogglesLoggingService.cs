using System;

namespace MogglesClient.PublicInterface
{
    public interface IMogglesLoggingService
    {
        void TrackException(Exception ex, string application, string environment);
        void TrackException(Exception ex, string customMessage, string application, string environment);
        void TrackEvent(string eventName, string application, string environment);
    }
}
