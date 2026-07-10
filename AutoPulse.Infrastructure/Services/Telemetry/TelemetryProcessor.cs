using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Application.Application.Common.Telemetry.Dto;
using System.Globalization;

namespace AutoPulse.Infrastructure.Services.Telemetry
{
    public class TelemetryProcessor : ITelemetryProcessor
    {
        public void NaiveProcessTelemtry(string csvLine)
        {
            var parts = csvLine.Split(';');

            var telemetryDataDto = new TelemetryDataDto
            (
                parts[0].AsMemory(),
                double.Parse(parts[1], CultureInfo.InvariantCulture),
                double.Parse(parts[2], CultureInfo.InvariantCulture),
                double.Parse(parts[3], CultureInfo.InvariantCulture),
                DateTime.Parse(parts[4], CultureInfo.InvariantCulture)
            );
        }

        public void SpanProcessTelemetry(string csvLine)
        {
            ReadOnlySpan<char> span = csvLine.AsSpan();
            int firstSemi = span.IndexOf(";");
            if (firstSemi == -1) return;

            ReadOnlyMemory<char> vehicleMemory = csvLine.AsMemory(0, firstSemi);

            ReadOnlySpan<char> remaining = span.Slice(firstSemi + 1);
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
                vehicleMemory,
                double.Parse(latSpan, CultureInfo.InvariantCulture),
                double.Parse(lonSpan, CultureInfo.InvariantCulture),
                double.Parse(speedSpan, CultureInfo.InvariantCulture),
                DateTime.Parse(dateSpan, CultureInfo.InvariantCulture)
            );
        }
    }
}
