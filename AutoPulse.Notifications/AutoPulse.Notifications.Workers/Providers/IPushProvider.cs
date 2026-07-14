namespace AutoPulse.Notifications.Workers.Providers
{
    public interface IPushProvider
    {
        string ProviderName { get; }
        Task SendAsync(string to, string message, CancellationToken cancellationToken);
    }
}
