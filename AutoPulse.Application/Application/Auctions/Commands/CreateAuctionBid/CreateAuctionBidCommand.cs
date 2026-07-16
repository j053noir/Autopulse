using System.ComponentModel.DataAnnotations;
using AutoPulse.Application.Application.Common.Interfaces;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Commands.CreateAuctionBid
{
    public record CreateAuctionBidCommand
    (
        Guid AuctionId,
        Guid AuctioneerId,
        [Range(0.01, 1000000000000.0, ErrorMessage = "Amount must be greater than 0.")]
        decimal Amount,
        [RegularExpression(@"^(USD|CAD|COP)$", ErrorMessage = "Currency must be USD, CAD, or COP.")]
        string Currency,
        Guid IdempotencyKey
    ) : IRequest<Guid>, IIdempotentCommand;
}
