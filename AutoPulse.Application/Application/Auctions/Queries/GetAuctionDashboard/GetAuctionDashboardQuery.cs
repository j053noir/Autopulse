using AutoPulse.Application.Application.Auctions.Queries.GetAuctionDashboard.Dto;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Queries.GetAuctionDashboard
{
    public record GetAuctionDashboardQuery(Guid AuctionId) : IRequest<AuctionDashboardDto?>;
}
