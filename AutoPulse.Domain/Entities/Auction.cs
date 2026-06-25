using AutoPulse.Domain.Common;
using AutoPulse.Domain.ValueObjects;

namespace AutoPulse.Domain.Entities
{
    public class Auction: BaseEntity, IAggregateRoot
    {
        // Properties with private setters to prevent external manipulation
        public Guid VehicleId { get; private set; }
        public Vehicle? Vehicle { get; private set; }
        public Money? StartingPrice { get; private set; }
        public Money? CurrentPrice { get; private set; }
        public DateTime? EndTime { get; private set; }
        public bool? IsActive { get; private set; }

        // Concurrency Token
        public uint RowVersion { get; private set; }

        // Encapsulated collection: readonly for external entities
        private readonly List<Bid> _bids = new();
        public IReadOnlyCollection<Bid> Bids => _bids.AsReadOnly();

        public Auction () { }

        private Auction(Guid id, Vehicle vehicle, Money startingPrice, DateTime endTime)
        {
            Id = id;
            Vehicle = vehicle;
            StartingPrice = startingPrice;
            CurrentPrice = startingPrice with { };
            EndTime = endTime;
            IsActive = true;
        }

        public static Auction Create(Guid id, Vehicle vehicle, Money startingPrice, DateTime endTime)
        {
            ArgumentNullException.ThrowIfNull(vehicle);
            ArgumentNullException.ThrowIfNull(startingPrice);

            if (endTime <= DateTime.UtcNow)
                throw new ArgumentException("End time must be in the future.", nameof(endTime));

            return new Auction(id, vehicle, startingPrice, endTime);
        }

        public Bid PlaceBid(Guid bidderId, Money bidAmount)
        {
            if ((IsActive.HasValue && !IsActive.Value) || DateTime.UtcNow >= EndTime)
                throw new InvalidOperationException("Cannot place a bid on a closed or expired auction");

            if (CurrentPrice == null || bidAmount < CurrentPrice)
                throw new InvalidOperationException("The bid amount must be higher than the current price");

            var bid = Bid.Create(this, bidderId, bidAmount, DateTimeOffset.UtcNow);
            _bids.Add(bid);
            CurrentPrice = bidAmount;

            return bid;
        }
    }
}
