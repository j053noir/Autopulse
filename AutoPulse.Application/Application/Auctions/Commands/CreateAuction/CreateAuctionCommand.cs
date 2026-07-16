using System.ComponentModel.DataAnnotations;
using AutoPulse.Application.Application.Common.Interfaces;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Commands.CreateAuction
{
    public record CreateAuctionCommand
    (
        // Vehicle,
        string Vin,
        string Marquee,
        string Model,
        int Year,
        int Mileage,
        // Auction,
        Guid AuctioneerId,
        [Range(0.01, 1000000000000.0, ErrorMessage = "Starting price must be greater than 0.")]
        decimal StartingPrice,
        [RegularExpression(@"^(USD|CAD|COP)$", ErrorMessage = "Currency must be USD, CAD, or COP.")]
        string Currency,
        DateTime EndTime,
        Guid IdempotencyKey
    ) : IRequest<Guid>, IIdempotentCommand;
}
