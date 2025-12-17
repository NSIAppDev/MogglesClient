namespace MogglesClient.PublicInterface.Notifications
{
    public interface INotificationService
    {
        void TryNotifyMissingFeatureToggle(string featureFlagName);

        void TryNotifyBadAuthentication(string errorMessage);

        void TryNotifyMissingFeatureToggleUsingWorkflows(string featureFlagName);

        void TryNotifyBadAuthenticationUsingWorkflows(string errorMessage);
    }
}