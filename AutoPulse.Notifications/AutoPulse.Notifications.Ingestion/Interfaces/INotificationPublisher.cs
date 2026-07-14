using AutoPulse.Notifications.Shared.Contracts;

namespace AutoPulse.Notifications.Ingestion.Interfaces
{
    public interface INotificationPublisher
    {
        Task PublishAsync(NotificationEventPayload payload, CancellationToken cancellationToken = default);
    }
}
