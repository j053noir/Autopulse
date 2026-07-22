using AutoPulse.Application.Application.Common.Interfaces;
using System.Net.Http.Json;

namespace AutoPulse.Infrastructure.Services
{
    public class NotificationHttpClient : INotificationClient
    {
        private readonly HttpClient _httpClient;

        public NotificationHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendAuctionCreatedNotificationAsync(
            Guid userId,
            Guid auctionId,
            string priority,
            string channel,
            string target,
            string templateId,
            Dictionary<string, string> templateVariables,
            CancellationToken cancellationToken = default)
        {
            var requestPayload = new
            {
                UserId = userId,
                AuctionId = auctionId,
                Priority = priority,
                Channel = channel,
                Target = target,
                TemplateId = templateId,
                TemplateVariables = templateVariables
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/notifications", requestPayload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
