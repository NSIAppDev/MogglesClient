using MogglesClient.PublicInterface.Notifications;
using System;

namespace MogglesClient.PublicInterface.NotificationsCache
{
    public interface INotificationsCache
    {
        bool NotificationExists(Message message);

        void CacheNotification(Message message, DateTimeOffset absoluteExpiration);

        bool NotificationExists(WorkflowMessage message);

        void CacheNotification(WorkflowMessage message, DateTimeOffset absoluteExpiration);
    }
}
