using AutoPulse.Application.Application.Common.Telemetry.Dto;

namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface ITelemetryProcessor
    {
        void NaiveProcessTelemtry(string csvLine);
        void SpanProcessTelemetry(ReadOnlySpan<char> csvLine);
    }
}
