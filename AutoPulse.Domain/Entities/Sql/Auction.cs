using AutoPulse.Domain.Common;
using AutoPulse.Domain.ValueObjects;

namespace AutoPulse.Domain.Entities
{
    public class Auction : BaseEntity, IAggregateRoot
    {
        // Properties with private setters to prevent external manipulation
        public Guid VehicleId { get; private set; }
        public Vehicle? Vehicle { get; private set; }
        public Money? StartingPrice { get; private set; }
        public Money? CurrentPrice { get; private set; }
        public DateTime? EndTime { get; private set; }
        public bool? IsActive { get; private set; }
        public Guid AuctioneerId { get; private set; }
        public User? Auctioneer { get; private set; }
        public Guid? WinnerId { get; private set; }
        public User? Winner { get; private set; }

        // Concurrency Token
        public uint RowVersion { get; private set; }

        // Encapsulated collection: readonly for external entities
        private readonly List<Bid> _bids = new();
        public IReadOnlyCollection<Bid> Bids => _bids.AsReadOnly();

        public Auction() { }

        private Auction(Guid id, Guid auctioneerId, Vehicle vehicle, Money startingPrice, DateTime endTime)
        {
            Id = id;
            Vehicle = vehicle;
            StartingPrice = startingPrice;
            CurrentPrice = startingPrice with { };
            EndTime = endTime;
            AuctioneerId = auctioneerId;
            IsActive = true;
        }

        public static Auction Create(Guid id, Guid auctioneerId, Vehicle vehicle, Money startingPrice, DateTime endTime)
        {
            ArgumentNullException.ThrowIfNull(vehicle);
            ArgumentNullException.ThrowIfNull(startingPrice);

            if (endTime <= DateTime.UtcNow)
                throw new ArgumentException("End time must be in the future.", nameof(endTime));

            var auction = new Auction(id, auctioneerId, vehicle, startingPrice, endTime);
            
            auction.AddDomainEvent(new Events.AuctionCreatedDomainEvent(
                auction.Id,
                vehicle.Title ?? string.Empty,
                startingPrice.Amount,
                endTime,
                auctioneerId
            ));

            return auction;
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

        public void Close()
        {
            if (IsActive.HasValue && !IsActive.Value)
                throw new InvalidOperationException("Auction is already closed");
            IsActive = false;

            if (_bids.Count > 0)
            {
                var winnderId = _bids.OrderByDescending(b => b.Amount).FirstOrDefault()?.BidderId;
                if (winnderId is not null) WinnerId = winnderId;
            }
        }

        public void Reopen()
        {
            IsActive = true;
            WinnerId = null;
            EndTime = DateTime.UtcNow.AddDays(1);
        }
    }
}
