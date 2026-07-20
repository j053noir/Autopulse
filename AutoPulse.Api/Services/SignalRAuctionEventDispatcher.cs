using AutoPulse.Api.Hubs;
using AutoPulse.Api.Hubs.Interfaces;
using AutoPulse.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AutoPulse.Api.Services
{
    public class SignalRAuctionEventDispatcher : IAuctionEventDispatcher
    {
        private readonly IHubContext<AuctionHub, ISignalRActionClient> _hubContext;

        public SignalRAuctionEventDispatcher(IHubContext<AuctionHub, ISignalRActionClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PublishAuctionEndedAsync(string auctionId, string winnerName)
        {
            await _hubContext.Clients.Group(auctionId).OnAuctionEnded(auctionId, winnerName);
        }

        public async Task PublishBidPlaceAsync(string auctionId, decimal newPrice, string bidderName)
        {
            await _hubContext.Clients.Group(auctionId).OnBidPlaced(auctionId, newPrice, bidderName);
        }
    }
}
