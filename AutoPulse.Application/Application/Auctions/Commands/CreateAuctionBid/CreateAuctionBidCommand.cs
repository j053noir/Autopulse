using AutoPulse.Application.Application.Common.Interfaces;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Commands.CreateAuctionBid
{
    public record CreateAuctionBidCommand
    (
        Guid AuctionId,
        Guid AuctioneerId,
        decimal Amount,
        Guid IdempotencyKey
    ) : IRequest<Guid>, IIdempotentCommand;
}
