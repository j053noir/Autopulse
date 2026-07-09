using AutoPulse.Application.Application.Auctions.Queries.GetAuctionDashboard.Dto;

namespace AutoPulse.Application.Application.Common.Interfaces.Queries
{
    public interface IAuctionQueries
    {
        Task<AuctionDashboardDto?> GetAuctionDashboardAsync(Guid auctionId, CancellationToken cancellationToken = default);
    }
}
