using AutoPulse.Notifications.Workers.Providers;

namespace AutoPulse.Notifications.Workers.Infrastructure.Providers.Email
{
    public class SendGridEmailProvider : IEmailProvider
    {
        private readonly HttpClient _httpClient;
        public string ProviderName => "SendGrid";


        public SendGridEmailProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsync("/v3/mail/send", null, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"SendGrid Error: {response.StatusCode}");
            }
        }
    }
}
