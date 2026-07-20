namespace AutoPulse.Domain.Interfaces
{
    public interface IAuctionEventDispatcher
    {
        Task PublishBidPlaceAsync(string auctionId, decimal newPrice, string bidderName);
        Task PublishAuctionEndedAsync(string auctionId, string winnerName);
    }
}
