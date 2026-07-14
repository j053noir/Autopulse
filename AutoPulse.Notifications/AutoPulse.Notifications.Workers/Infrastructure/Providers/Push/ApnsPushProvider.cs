using AutoPulse.Notifications.Workers.Providers;

namespace AutoPulse.Notifications.Workers.Infrastructure.Providers.Push
{
    public class ApnsPushProvider : IPushProvider
    {
        public string ProviderName => "APNs";

        public async Task SendAsync(string deviceToken, string title, string body, CancellationToken cancellationToken)
        {
            // TODO: Push notificaiton logic
            await Task.Delay(35, cancellationToken);
        }
    }
}
