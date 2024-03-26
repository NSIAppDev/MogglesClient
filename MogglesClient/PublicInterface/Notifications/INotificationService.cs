namespace MogglesClient.PublicInterface.Notifications
{
    public interface INotificationService
    {
        void TryNotifyMissingFeatureToggle(string featureFlagName);

        void TryNotifyBadAuthentication(string errorMessage);
    }
}