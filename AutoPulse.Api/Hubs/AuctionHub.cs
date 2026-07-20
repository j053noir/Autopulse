using AutoPulse.Api.Hubs.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AutoPulse.Api.Hubs
{
    public class AuctionHub : Hub<ISignalRActionClient>
    {
        public async Task JoinAuctionRoom(string auctioId) =>
            await Groups.AddToGroupAsync(Context.ConnectionId, auctioId);

        public async Task LeaveAuctionRoom(string auctionId) =>
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId);
    }
}
