using AutoPulse.Application.Application.Common.Interfaces;
using MassTransit;

namespace AutoPulse.Infrastructure.Messaging
{
    public class EventBus : IEventBus
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public EventBus(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default) 
            where TEvent : class, IIntegrationEvent
        {
            await _publishEndpoint.Publish(evt, cancellationToken);
        }
    }
}
