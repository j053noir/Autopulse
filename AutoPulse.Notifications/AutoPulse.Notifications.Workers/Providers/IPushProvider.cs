namespace AutoPulse.Notifications.Workers.Providers
{
    public interface IPushProvider
    {
        string ProviderName { get; }
        Task SendAsync(string deviceToken, string title, string body, CancellationToken cancellationToken);
    }
}
