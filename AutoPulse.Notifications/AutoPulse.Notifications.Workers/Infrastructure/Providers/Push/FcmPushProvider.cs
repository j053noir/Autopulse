using AutoPulse.Notifications.Workers.Providers;

namespace AutoPulse.Notifications.Workers.Infrastructure.Providers.Push
{
    public class FcmPushProvider : IPushProvider
    {
        public string ProviderName => "FCM";

        public async Task SendAsync(string deviceToken, string title, string body, CancellationToken cancellationToken)
        {
            // TODO: Push notificaiton logic
            await Task.Delay(30, cancellationToken);
        }
    }
}
