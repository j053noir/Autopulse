using System;

namespace AutoPulse.Application.Application.Common.Telemetry.Dto
{
    public readonly record struct TelemetryDataDto
    (
        ReadOnlyMemory<char> VehicleId,
        double Latitude,
        double Longitude,
        double Speed,
        DateTime Timestamp
    );
}
