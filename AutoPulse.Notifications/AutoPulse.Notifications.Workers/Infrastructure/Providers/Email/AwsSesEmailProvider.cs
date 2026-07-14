using AutoPulse.Notifications.Workers.Providers;

namespace AutoPulse.Notifications.Workers.Infrastructure.Providers.Email
{
    public class AwsSesEmailProvider : IEmailProvider
    {
        public string ProviderName => "AwsSes";

        public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            await Task.Delay(50, cancellationToken);
        }
    }
}
