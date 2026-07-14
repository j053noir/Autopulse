using AutoPulse.Notifications.Workers.Providers;

namespace AutoPulse.Notifications.Workers.Infrastructure.Providers
{
    internal class TwilioSmsProvider : ISmsProvider
    {
        public string ProviderName => nameof(TwilioSmsProvider);

        public Task SendSmsAsync(string to, string message, CancellationToken cancellationToken)
        {
            // TODO: Implement twilio
            throw new NotImplementedException();
        }
    }
}
