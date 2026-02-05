#if NETCORE
using Microsoft.Extensions.Caching.Memory;
using MogglesClient.PublicInterface.Notifications;
using System;

namespace MogglesClient.PublicInterface.NotificationsCache
{
    public class NetCoreNotificationsCache : INotificationsCache
    {
        private MemoryCache Cache { get; set; }

        public NetCoreNotificationsCache()
        {
            Cache = new MemoryCache(new MemoryCacheOptions());
        }

        public void CacheNotification(Message message, DateTimeOffset absoluteExpiration)
        {
            Cache.Set(message.Title, message, absoluteExpiration);
        }

        public bool NotificationExists(Message message)
        {
            return Cache.Get(message.Title) != null;
        }
    }
}
#endif