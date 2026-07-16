using System.Diagnostics;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Application.Application.Vehicles.Commands.ProcessTelemetry;
using AutoPulse.Domain.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoPulse.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ITelemetryProcessor _telemetryProcessor;

        public TelemetryController(IMediator mediator, ITelemetryProcessor telemetryProcessor)
        {
            _mediator = mediator;
            _telemetryProcessor = telemetryProcessor;
        }

        /// <summary>
        /// Processing of telemetry data comes in raw format from the car sensors.
        /// </summary>
        /// <param name="method">Method used for processing telemetry data. Possible values: span, naive.</param>
        /// <param name="rawData">Raw telemetry data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = Permissions.Telemetry.Process)]
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

        [HttpPost("benchmark")]
        [Authorize(Policy = Permissions.Telemetry.Benchmark)]
        public IActionResult Benchmark([FromBody] string rawData)
        {
            const int iterations = 500_000;

            // --- 1. BENCHMARK NAIVE ---
            GC.Collect(0, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();
            long beforeNaiveGen0 = GC.CollectionCount(0);
            long beforeNaiveGen1 = GC.CollectionCount(1);
            long beforeNaiveGen2 = GC.CollectionCount(2);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _telemetryProcessor.NaiveProcessTelemetry(rawData);
            }
            sw.Stop();
            double naiveTimeMs = sw.Elapsed.TotalMilliseconds;
            long naiveGen0 = GC.CollectionCount(0) - beforeNaiveGen0;
            long naiveGen1 = GC.CollectionCount(1) - beforeNaiveGen1;
            long naiveGen2 = GC.CollectionCount(2) - beforeNaiveGen2;

            // --- 2. BENCHMARK SPAN ---
            GC.Collect(0, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();
            long beforeSpanGen0 = GC.CollectionCount(0);
            long beforeSpanGen1 = GC.CollectionCount(1);
            long beforeSpanGen2 = GC.CollectionCount(2);

            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                _telemetryProcessor.SpanProcessTelemetry(rawData);
            }
            sw.Stop();
            double spanTimeMs = sw.Elapsed.TotalMilliseconds;
            long spanGen0 = GC.CollectionCount(0) - beforeSpanGen0;
            long spanGen1 = GC.CollectionCount(1) - beforeSpanGen1;
            long spanGen2 = GC.CollectionCount(2) - beforeSpanGen2;

            var results = new
            {
                Iterations = iterations,
                Naive = new
                {
                    TimeMs = naiveTimeMs,
                    Gen0Collections = naiveGen0,
                    Gen1Collections = naiveGen1,
                    Gen2Collections = naiveGen2
                },
                Span = new
                {
                    TimeMs = spanTimeMs,
                    Gen0Collections = spanGen0,
                    Gen1Collections = spanGen1,
                    Gen2Collections = spanGen2
                },
                Improvement = new
                {
                    TimeSavingPercent = Math.Round((naiveTimeMs - spanTimeMs) / naiveTimeMs * 100, 2),
                    GarbageReductionPercent = naiveGen0 > 0
                        ? Math.Round((double)(naiveGen0 - spanGen0) / naiveGen0 * 100, 2)
                        : 0
                }
            };

            return Ok(results);
        }
    }
}
