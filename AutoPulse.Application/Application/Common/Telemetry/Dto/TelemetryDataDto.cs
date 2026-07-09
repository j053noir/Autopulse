namespace AutoPulse.Application.Application.Common.Telemetry.Dto
{
    public record TelemetryDataDto
    (
        string VehicleId,
        double Latitude,
        double Longitude,
        double Speed,
        DateTime Timestamp
    );
}
