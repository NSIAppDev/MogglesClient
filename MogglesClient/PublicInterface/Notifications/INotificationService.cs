namespace MogglesClient.PublicInterface.Notifications
{
    public interface INotificationService
    {
        void TryNotifyMissingFeatureToggle(string featureFlagName);
    }
}