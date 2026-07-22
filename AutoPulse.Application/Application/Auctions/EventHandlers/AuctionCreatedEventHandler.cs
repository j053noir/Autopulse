using AutoPulse.Application.Application.Common.Events;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Application.Application.Auctions.EventHandlers
{
    public class AuctionCreatedEventHandler : INotificationHandler<AuctionCreatedEvent>
    {
        private readonly IAuctionEventDispatcher _auctionEventDispatcher;
        private readonly INotificationClient _notificationClient;
        private readonly ILogger<AuctionCreatedEventHandler> _logger;

        public AuctionCreatedEventHandler(
            IAuctionEventDispatcher auctionEventDispatcher,
            INotificationClient notificationClient,
            ILogger<AuctionCreatedEventHandler> logger)
        {
            _auctionEventDispatcher = auctionEventDispatcher;
            _notificationClient = notificationClient;
            _logger = logger;
        }

        public async Task Handle(AuctionCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling AuctionCreatedDomainEvent for Auction: {AuctionId}", notification.AuctionId);

            // 1. Send SignalR real-time event
            try
            {
                await _auctionEventDispatcher.PublishAuctionCreatedAsync(
                    notification.AuctionId.ToString(),
                    notification.Title,
                    notification.BasePrice
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish SignalR OnAuctionCreated for Auction: {AuctionId}", notification.AuctionId);
            }

            // 2. Call Notifications Ingestion API
            try
            {
                var templateVariables = new Dictionary<string, string>
                {
                    { "AuctionId", notification.AuctionId.ToString() },
                    { "Title", notification.Title },
                    { "BasePrice", notification.BasePrice.ToString("F2") },
                    { "EndTime", notification.EndTime.ToString("o") }
                };

                await _notificationClient.SendAuctionCreatedNotificationAsync(
                    notification.AuctioneerId,
                    notification.AuctionId,
                    priority: "transactional",
                    channel: "email",
                    target: "auctioneer@autopulse.com", // Fallback or retrieve actual user email if needed
                    templateId: "AuctionCreatedTemplate",
                    templateVariables: templateVariables,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send ingestion notification for Auction: {AuctionId}", notification.AuctionId);
            }
        }
    }
}
