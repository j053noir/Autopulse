using AutoPulse.Application.Application.Common.Interfaces;
using MediatR;

namespace AutoPulse.Application.Application.Vehicles.Commands.UpdateVehicleSpecs
{
    public record UpdateVehicleSpecsCommand
    (
        string AuctionId,
        string Key,
        object Value,
        Guid IdempotencyKey
    ) : IRequest<bool>, IIdempotentCommand;
}
