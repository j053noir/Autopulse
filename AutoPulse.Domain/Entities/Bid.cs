using AutoPulse.Domain.Common;
using AutoPulse.Domain.ValueObjects;

namespace AutoPulse.Domain.Entities
{
    public class Bid : BaseEntity
    {
        public Guid AuctionId { get; private set; }
        public Auction? Auction { get; private set; }
        public Guid BidderId { get; private set; }
        public Money? Amount { get; private set; }

        public Bid() { }

        private Bid(Guid id, Guid auctionId, Guid bidderId, Money amount, DateTimeOffset? createdAt)
        {
            Id = id;
            AuctionId = auctionId;
            BidderId = bidderId;
            Amount = amount;
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        }

        public static Bid Create(Auction auction, Guid bidderId, Money amount, DateTimeOffset? createdAt)
        {
            if (bidderId == Guid.Empty) throw new ArgumentException("bidderId is required");
            if (amount == null || amount.Amount < 0) throw new ArgumentException("amount is required");

            return new Bid(Guid.Empty, auction.Id, bidderId, amount, createdAt);
        }
    }
}
