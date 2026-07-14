using AutoPulse.Notifications.Shared.Contracts;

namespace AutoPulse.Notifications.Workers.Interfaces
{
    public interface INotificationDispatcher
    {
        Task DispatchAsync(NotificationEventPayload notificationPayload, CancellationToken cancellationToken = default);
    }
}
