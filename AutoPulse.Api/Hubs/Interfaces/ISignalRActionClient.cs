namespace AutoPulse.Api.Hubs.Interfaces
{
    public interface ISignalRActionClient
    {
        Task OnBidPlaced(string auctionId, decimal newPrice, string bidderName);
        Task OnAuctionEnded(string auctionId, string winnerName);
        Task OnAuctionCreated(string auctionId, string title, decimal basePrice);
    }
}
