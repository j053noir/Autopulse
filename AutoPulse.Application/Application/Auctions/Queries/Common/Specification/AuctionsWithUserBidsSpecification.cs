using AutoPulse.Domain.Common.Specification;
using AutoPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoPulse.Application.Application.Auctions.Queries.Common.Specification
{
    public class AuctionsWithUserBidsSpecification : BaseSpecification<Auction>
    {
        public AuctionsWithUserBidsSpecification(Guid userId) :
            base(a => a.Bids.Any(b => b.BidderId == userId))
        {
            AddInclude(a => a.Vehicle);
            AddInclude(q => q.Include(a => a.Bids).ThenInclude(b => b.Bidder!));
        }
    }
}
