using AutoPulse.Notifications.Workers.Providers;

namespace AutoPulse.Notifications.Workers.Infrastructure.Providers
{
    internal class InfobipSmsProvider : ISmsProvider
    {
        public string ProviderName => nameof(InfobipSmsProvider);

        public Task SendSmsAsync(string to, string message, CancellationToken cancellationToken)
        {
            // TODO: Implement infobip
            throw new NotImplementedException();
        }
    }
}
