﻿#if NETCORE
using System;
using MassTransit;
using MogglesClient.Messaging.RefreshCache;
using MogglesClient.PublicInterface;
using MogglesContracts;

namespace MogglesClient.Messaging
{
    public class NetCoreMogglesBusService : IMogglesBusService
    {
        private readonly IMogglesConfigurationManager _mogglesConfigurationManager;
        private IBusControl _busControl;

        public NetCoreMogglesBusService(IMogglesConfigurationManager mogglesConfigurationManager)
        {
            _mogglesConfigurationManager = mogglesConfigurationManager;
        }

        public void ConfigureAndStartMessageBus()
        {
            _busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(_mogglesConfigurationManager.GetMessageBusUrl()), h =>
                {
                    h.Username(_mogglesConfigurationManager.GetMessageBusUser());
                    h.Password(_mogglesConfigurationManager.GetMessageBusPassword());
                });

                var cacheRefreshQueue = _mogglesConfigurationManager.GetCacheRefreshQueue();
                if (UseCustomQueue(cacheRefreshQueue))
                {
                    cfg.ReceiveEndpoint(cacheRefreshQueue, e =>
                    {
                        e.Consumer<ClearTogglesCacheConsumer>();
                    });
                }
                else
                {
                    cfg.ReceiveEndpoint(e =>
                    {
                        e.Consumer<ClearTogglesCacheConsumer>();
                    });
                }
            });

            _busControl.Start();

            bool UseCustomQueue(string cacheRefreshQueue)
            {
                return !string.IsNullOrEmpty(cacheRefreshQueue);
            }
        }

        public void Publish(RegisteredTogglesUpdate registeredTogglesUpdate)
        {
            _busControl.Publish(registeredTogglesUpdate);
        }
    }
}
#endif