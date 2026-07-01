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
        decimal StartingPrice,
        DateTime EndTime,
        Guid IdempotencyKey
    ) : IRequest<Guid>, IIdempotentCommand;
}
