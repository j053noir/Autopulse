namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface IIntegrationEvent
    {
        Guid EventId { get; }
        DateTime OccuredOn { get; }
    }

    public interface IEventBus
    {
        Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default) 
            where TEvent : class, IIntegrationEvent;
    }
}
