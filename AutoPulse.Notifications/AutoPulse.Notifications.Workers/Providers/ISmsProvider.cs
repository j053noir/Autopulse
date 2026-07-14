namespace AutoPulse.Notifications.Workers.Providers
{
    public interface ISmsProvider
    {
        string ProviderName { get; }
        Task SendSmsAsync(string to, string message, CancellationToken cancellationToken);
    }
}
