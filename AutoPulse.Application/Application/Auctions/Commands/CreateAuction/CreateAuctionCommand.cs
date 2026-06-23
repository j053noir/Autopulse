using AutoPulse.Application.Application.Common.Interfaces;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Commands.CreateAuction
{
    public record CreateAuctionCommand
    (
        string VehicleModel,
        decimal StartingPrice,
        DateTime EndTime,
        Guid IdempotencyKey
    ) : IRequest<Guid>, IIdempotentCommand;
}
