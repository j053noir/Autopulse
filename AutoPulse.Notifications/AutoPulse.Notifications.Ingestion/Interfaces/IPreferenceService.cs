namespace AutoPulse.Notifications.Ingestion.Interfaces
{
    public interface IPreferenceService
    {
        Task<bool> IsChannelAllowedAsync(string userId, string channelId, CancellationToken cancellationToken = default);
    }
}
