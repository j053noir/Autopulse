using AutoPulse.Domain.Common;
using AutoPulse.Domain.ValueObjects;

namespace AutoPulse.Domain.Entities
{
    public class Bid : IAggregateRoot
    {
        public Guid Id { get; private set; }
        public Guid? BidderId { get; private set; } = Guid.Empty;
        public Money? Amount { get; private set; }
        public DateTime? CreatedAt { get; private set; }

        public Bid() { }

        public Bid(Guid id, Guid bidderId, Money amount, DateTime? createdAt)
        {
            if (bidderId == Guid.Empty) throw new ArgumentException("bidderId is required");
            if (amount == null) throw new ArgumentException("amout is required");

            Id = id;
            BidderId = bidderId;
            Amount = amount;
            CreatedAt = createdAt ?? DateTime.UtcNow;
        }
    }
}
