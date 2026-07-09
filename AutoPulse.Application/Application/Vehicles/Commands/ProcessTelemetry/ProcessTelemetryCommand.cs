using MediatR;

namespace AutoPulse.Application.Application.Vehicles.Commands.ProcessTelemetry
{
    public enum ProcessTelemetryMethod
    {
        Naive = 0,
        Span = 1,
    }

    public record ProcessTelemetryCommand
    (
        string RawData,
        ProcessTelemetryMethod method = ProcessTelemetryMethod.Naive
    ) : IRequest<bool>;
}
