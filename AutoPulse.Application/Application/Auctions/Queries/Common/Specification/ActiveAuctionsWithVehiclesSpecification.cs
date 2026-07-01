using AutoPulse.Domain.Common.Specification;
using AutoPulse.Domain.Entities;

namespace AutoPulse.Application.Application.Auctions.Queries.Common.Specification
{
    public class ActiveAuctionsWithVehiclesSpecification : BaseSpecification<Auction>
    {
        public ActiveAuctionsWithVehiclesSpecification(
            Guid? auctionId = null,
            Guid? auctioneerId = null,
            string? marquee = null,
            int? minYear = null,
            decimal? maxPrice = null
        ) :
            base(
                a => a.IsActive.Value && a.EndTime > DateTime.UtcNow &&
                    (auctionId == Guid.Empty || a.Id == auctionId) &&
                    (auctioneerId == Guid.Empty || a.AuctioneerId == auctioneerId) &&
                    (marquee == null || a.Vehicle.Marquee.ToLower() == marquee.ToLower()) &&
                    (minYear == null || a.Vehicle.Year >= minYear) &&
                    (maxPrice == null || a.CurrentPrice.Amount <= maxPrice)
            )
        {
            AddInclude(a => a.Auctioneer);
            AddInclude(a => a.Vehicle);
            ApplyOrderByDescending(a => a.CurrentPrice.Amount);
        }
    }
}
