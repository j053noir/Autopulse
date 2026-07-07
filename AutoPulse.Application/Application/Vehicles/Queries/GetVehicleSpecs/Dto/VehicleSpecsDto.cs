namespace AutoPulse.Application.Application.Vehicles.Queries.GetVehicleSpecs.Dto
{
    public record VehicleSpecsDto(
        string Model,
        string Brand,
        Dictionary<string, object> Specifications
    );
}
