using AutoPulse.Domain.Common.Specification;
using AutoPulse.Domain.Entities;

namespace AutoPulse.Application.Application.Auctions.Queries.Common.Specification
{
    public class AuctionWithDetailsSpecification : BaseSpecification<Auction>
    {
        public AuctionWithDetailsSpecification(Guid auctionId) :
            base(a => a.Id == auctionId)
        {
            AddInclude(a => a.Auctioneer);
            AddInclude(a => a.Vehicle);
            AddInclude(a => a.Bids);
        }
    }
}
