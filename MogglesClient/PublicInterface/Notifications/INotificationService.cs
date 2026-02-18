using System.Threading.Tasks;

namespace MogglesClient.PublicInterface.Notifications
{
    public interface INotificationService
    {
        Task TryNotifyMissingFeatureToggle(string featureFlagName);

        Task TryNotifyBadAuthentication(string errorMessage);
    }
}