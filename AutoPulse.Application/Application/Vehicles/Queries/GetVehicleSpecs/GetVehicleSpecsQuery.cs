using AutoPulse.Application.Application.Vehicles.Queries.GetVehicleSpecs.Dto;
using MediatR;

namespace AutoPulse.Application.Application.Vehicles.Queries.GetVehicleSpecs
{
    public record GetVehicleSpecsQuery(string AuctionId) : IRequest<VehicleSpecsDto?>;
}
