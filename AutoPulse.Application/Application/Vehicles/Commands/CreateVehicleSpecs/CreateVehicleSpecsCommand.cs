using AutoPulse.Application.Application.Common.Interfaces;
using MediatR;
using System.Collections.ObjectModel;

namespace AutoPulse.Application.Application.Vehicles.Commands.CreateVehicleSpecs
{
    public record CreateVehicleSpecsCommand(
        string AuctionId,
        ReadOnlyDictionary<string, object> KeyValuePairs,
        Guid IdempotencyKey
    ) : IRequest<string>, IIdempotentCommand;
}
