using AutoPulse.Application.Application.Common.Interfaces;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Commands.CreateAuctionBid
{
    public record CreateAuctionBidCommand
    (
        Guid auctionId,
        Guid bidderId,
        decimal amount,
        Guid IdempotencyKey
    ) : IRequest<Guid>, IIdempotentCommand;
}
