using AutoPulse.Notifications.Workers.Providers;

namespace AutoPulse.Notifications.Workers.Infrastructure.Providers.Sms
{
    internal class InfobipSmsProvider : ISmsProvider
    {
        public string ProviderName => "Infobip";

        public Task SendSmsAsync(string to, string message, CancellationToken cancellationToken)
        {
            // TODO: Implement infobip
            throw new NotImplementedException();
        }
    }
}
