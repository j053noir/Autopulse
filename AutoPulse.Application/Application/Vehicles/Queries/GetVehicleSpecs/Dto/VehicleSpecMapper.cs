using AutoPulse.Domain.Entities.NoSql;

namespace AutoPulse.Application.Application.Vehicles.Queries.GetVehicleSpecs.Dto
{
    public static class VehicleSpecMapper
    {
        public static VehicleSpecsDto Map(VehicleSpecificationDocument vehicleSpecs)
        {
            return new VehicleSpecsDto(
                vehicleSpecs.Model,
                vehicleSpecs.Brand,
                vehicleSpecs.DynamicMetadata.ToDictionary()
            );
        }
    }
}
