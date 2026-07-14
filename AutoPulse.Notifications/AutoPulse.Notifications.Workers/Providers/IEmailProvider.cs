namespace AutoPulse.Notifications.Workers.Providers
{
    public interface IEmailProvider
    {
        string ProviderName { get; }
        Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    }
}
