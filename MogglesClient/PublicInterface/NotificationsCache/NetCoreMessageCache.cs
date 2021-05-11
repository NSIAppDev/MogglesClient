#if NETCORE
using MogglesClient.PublicInterface.Notifications;
using System;
using Microsoft.Extensions.Caching.Memory;

namespace MogglesClient.PublicInterface.NotificationsCache
{
    public class NetCoreNotificationsCache: INotificationsCache
    {
        private MemoryCache Cache { get; set; }

        public NetCoreNotificationsCache()
        {
            Cache = new MemoryCache(new MemoryCacheOptions());
        }

        public void CacheNotification(Message message, DateTimeOffset absoluteExpiration)
        {
            Cache.Set(message.text, message, absoluteExpiration);
        }

        public bool NotificationExists(Message message)
        {
            return Cache.Get(message.text) != null;
        }
    }
}
#endif