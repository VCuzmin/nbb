﻿using NBB.Core.Abstractions;
using NBB.Correlation;
using NBB.EventStore.Abstractions;
using NBB.EventStore.MessagingExtensions.Internal;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.MessagingExtensions
{
    public class MessagingEventStoreSubscriber : IEventStoreSubscriber
    {
        private readonly IMessageBusSubscriber<IEvent> _messageBusSubscriber;
        private readonly MessagingTopicResolver _messagingTopicResolver;
        private readonly MessagingSubscriberOptions _subscriberOptions;

        public MessagingEventStoreSubscriber(IMessageBusSubscriber<IEvent> messageBusSubscriber, MessagingTopicResolver messagingTopicResolver, MessagingSubscriberOptions subscriberOptions)
        {
            _messageBusSubscriber = messageBusSubscriber;
            _messagingTopicResolver = messagingTopicResolver;
            _subscriberOptions = subscriberOptions;
        }


        public Task SubscribeToAllAsync(Func<IEvent, Task> handler, CancellationToken cancellationToken = default)
        {
            return _messageBusSubscriber.SubscribeAsync(async envelope =>
            {
                using (CorrelationManager.NewCorrelationId(envelope.GetCorrelationId()))
                {
                    await handler(envelope.Payload as IEvent);
                }
            }, cancellationToken, _messagingTopicResolver.ResolveTopicName(), _subscriberOptions);
        }
    }
}
