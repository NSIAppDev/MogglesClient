#if NETFULL
using MogglesClient.PublicInterface.Notifications;
using System;
using System.Runtime.Caching;

namespace MogglesClient.PublicInterface.NotificationsCache
{
    public class NetFullNotificationsCache: INotificationsCache
    {
        private MemoryCache Cache { get; set; }

        public NetFullNotificationsCache()
        {
            Cache = new MemoryCache("NotificationsCache");
        }

        public void CacheNotification(Message message, DateTimeOffset absoluteExpiration)
        {   
            Cache.Add(message.text, message, absoluteExpiration);
        }

        public bool NotificationExists(Message message)
        {
            return Cache.Contains(message.text);
        }
    }
}
#endif