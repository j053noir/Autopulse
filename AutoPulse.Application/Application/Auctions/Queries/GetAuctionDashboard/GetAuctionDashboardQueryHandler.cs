using AutoPulse.Application.Application.Auctions.Queries.GetAuctionDashboard.Dto;
using AutoPulse.Application.Application.Common.Interfaces.Queries;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Queries.GetAuctionDashboard
{
    public class GetAuctionDashboardQueryHandler : IRequestHandler<GetAuctionDashboardQuery, AuctionDashboardDto?>
    {
        private readonly IAuctionQueries _auctionQueries;

        public GetAuctionDashboardQueryHandler(IAuctionQueries auctionQueries)
        {
            _auctionQueries = auctionQueries;
        }

        public async Task<AuctionDashboardDto?> Handle(GetAuctionDashboardQuery request, CancellationToken cancellationToken)
        {
            return await _auctionQueries.GetAuctionDashboardAsync(request.AuctionId, cancellationToken);
        }
    }
}
