namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface ITelemetryProcessor
    {
        void NaiveProcessTelemtry(string csvLine);
        void SpanProcessTelemetry(string csvLine);
    }
}
