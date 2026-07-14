using AutoPulse.Notifications.Workers.Providers;

namespace AutoPulse.Notifications.Workers.Infrastructure.Providers.Sms
{
    internal class TwilioSmsProvider : ISmsProvider
    {
        public string ProviderName => "Twilio";

        public Task SendSmsAsync(string to, string message, CancellationToken cancellationToken)
        {
            // TODO: Implement twilio
            throw new NotImplementedException();
        }
    }
}
