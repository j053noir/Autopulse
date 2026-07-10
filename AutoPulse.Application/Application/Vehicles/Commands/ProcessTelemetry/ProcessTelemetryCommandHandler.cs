using AutoPulse.Application.Application.Common.Interfaces;
using MediatR;

namespace AutoPulse.Application.Application.Vehicles.Commands.ProcessTelemetry
{
    internal class ProcessTelemetryCommandHandler : IRequestHandler<ProcessTelemetryCommand, bool>
    {
        private readonly ITelemetryProcessor _telemetryProcessor;

        public ProcessTelemetryCommandHandler(ITelemetryProcessor telemetryProcessor)
        {
            _telemetryProcessor = telemetryProcessor;
        }

        public Task<bool> Handle(ProcessTelemetryCommand request, CancellationToken cancellationToken)
        {
            if (request.method == ProcessTelemetryMethod.Span)
                _telemetryProcessor.SpanProcessTelemetry(request.RawData);
            else
                _telemetryProcessor.NaiveProcessTelemtry(request.RawData);

            return Task.FromResult(true);
        }
    }
}
