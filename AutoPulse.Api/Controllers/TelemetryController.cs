using AutoPulse.Application.Application.Vehicles.Commands.ProcessTelemetry;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoPulse.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : Controller
    {
        public readonly IMediator _mediator;

        public TelemetryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Processing of telemetry data comes in raw format from the car sensors.
        /// </summary>
        /// <param name="method">Method used for processing telemetry data. Possible values: span, naive.</param>
        /// <param name="rawData">Raw telemetry data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ProcessTelemetry
        (
            [FromQuery] string method,
            [FromBody] string rawData,
            CancellationToken cancellationToken = default
        )
        {
            var processingMethod = method == "span" ? ProcessTelemetryMethod.Span : ProcessTelemetryMethod.Naive;
            var result = await _mediator.Send(new ProcessTelemetryCommand(rawData, processingMethod), cancellationToken);

            return result ? Ok() : BadRequest();
        }
    }
}
