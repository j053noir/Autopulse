using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Application.Application.Common.Telemetry.Dto;
using System.Globalization;

namespace AutoPulse.Infrastructure.Services.Telemetry
{
    public class TelemetryProcessor : ITelemetryProcessor
    {
        public void NaiveProcessTelemtry(string csvLine)
        {
            var startDate = DateTime.UtcNow;
            Console.WriteLine("Start Naive processing telemetry date...");

            var parts = csvLine.Split(';');

            var telemetryDataDto = new TelemetryDataDto
            (
                parts[0],
                double.Parse(parts[1]),
                double.Parse(parts[2]),
                double.Parse(parts[3]),
                DateTime.Parse(parts[4])
            );

            var timeSpan = DateTime.UtcNow.Subtract(startDate);
            Console.WriteLine($"Finished processing telemetry data: {timeSpan.TotalMilliseconds}ms");
            Console.WriteLine(telemetryDataDto);
        }

        public void SpanProcessTelemetry(ReadOnlySpan<char> csvLine)
        {
            var startDate = DateTime.UtcNow;
            Console.WriteLine("Start Span processing telemetry date...");

            int firstSemi = csvLine.IndexOf(";");
            if (firstSemi == -1) return;

            ReadOnlySpan<char> vehicleSpan = csvLine.Slice(0, firstSemi);

            ReadOnlySpan<char> remaining = csvLine.Slice(firstSemi + 1);
            int secondSemi = remaining.IndexOf(";");
            ReadOnlySpan<char> latSpan = remaining.Slice(0, secondSemi);

            remaining = remaining.Slice(secondSemi + 1);
            int thirdSemi = remaining.IndexOf(";");
            ReadOnlySpan<char> lonSpan = remaining.Slice(0, thirdSemi);

            remaining = remaining.Slice(thirdSemi + 1);
            int fourthSemi = remaining.IndexOf(";");
            ReadOnlySpan<char> speedSpan = remaining.Slice(0, fourthSemi);

            ReadOnlySpan<char> dateSpan = remaining.Slice(fourthSemi + 1);

            var telemetryDataDto = new TelemetryDataDto
            (
                vehicleSpan.ToString(),
                double.Parse(latSpan, CultureInfo.InvariantCulture),
                double.Parse(lonSpan, CultureInfo.InvariantCulture),
                double.Parse(speedSpan, CultureInfo.InvariantCulture),
                DateTime.Parse(dateSpan, CultureInfo.InvariantCulture)
            );

            var timeSpan = DateTime.UtcNow.Subtract(startDate);
            Console.WriteLine($"Finished processing telemetry data: {timeSpan.TotalMilliseconds}ms");
            Console.WriteLine(telemetryDataDto);
        }
    }
}
