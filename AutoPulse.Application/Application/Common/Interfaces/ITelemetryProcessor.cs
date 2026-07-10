using AutoPulse.Application.Application.Common.Telemetry.Dto;

namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface ITelemetryProcessor
    {
        TelemetryDataDto? NaiveProcessTelemetry(string csvLine);
        TelemetryDataDto? SpanProcessTelemetry(string csvLine);
    }
}
