namespace AutoPulse.Domain.Common.Exceptions
{
    public class AuctionAlreadyClosedException : DomainException
    {
        public AuctionAlreadyClosedException(Guid auctionId)
        : base("Auction Closed", $"The auction with id '{auctionId}' has already closed and cannot accept new bids.", 409) { }
    }
}
