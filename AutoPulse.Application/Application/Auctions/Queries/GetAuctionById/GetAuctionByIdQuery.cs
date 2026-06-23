using MediatR;

namespace AutoPulse.Application.Application.Auctions.Queries.GetAuctionById
{
    public record GetAuctionByIdQuery(Guid Id) : IRequest<AuctionDto?>;
}
