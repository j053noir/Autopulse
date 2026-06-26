using AutoPulse.Domain.Common.Specification;
using AutoPulse.Domain.Entities;

namespace AutoPulse.Application.Application.Auctions.Queries.Common.Specification
{
    public class ActiveAuctionsWithVehiclesSpecification : BaseSpecification<Auction>
    {
        public ActiveAuctionsWithVehiclesSpecification(string? marquee, int? minYear, decimal? maxPrice) :
            base(
                a => a.IsActive.Value && a.EndTime > DateTime.UtcNow &&
                    (marquee == null || a.Vehicle.Marquee.ToLower() == marquee.ToLower()) &&
                    (minYear == null || a.Vehicle.Year >= minYear) &&
                    (maxPrice == null || a.CurrentPrice.Amount <= maxPrice)
            )
        {
            AddInclude(a => a.Vehicle);
            ApplyOrderByDescending(a => a.CurrentPrice.Amount);
        }
    }
}
