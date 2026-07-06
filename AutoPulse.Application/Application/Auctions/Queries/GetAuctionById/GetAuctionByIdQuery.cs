using AutoPulse.Application.Application.Auctions.Queries.Common.Dto;
using AutoPulse.Application.Application.Common.Interfaces;

namespace AutoPulse.Application.Application.Auctions.Queries.GetAuctionById
{
    public record GetAuctionByIdQuery(Guid Id) : IReadOnlyQuery<AuctionDto?>;
}
