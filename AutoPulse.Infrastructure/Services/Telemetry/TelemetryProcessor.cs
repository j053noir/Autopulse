using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Application.Application.Common.Telemetry.Dto;
using System.Globalization;

namespace AutoPulse.Infrastructure.Services.Telemetry
{
    public class TelemetryProcessor : ITelemetryProcessor
    {
        public TelemetryDataDto? NaiveProcessTelemetry(string csvLine)
        {
            var parts = csvLine.Split(';');

            return new TelemetryDataDto
            (
                parts[0].AsMemory(),
                double.Parse(parts[1], CultureInfo.InvariantCulture),
                double.Parse(parts[2], CultureInfo.InvariantCulture),
                double.Parse(parts[3], CultureInfo.InvariantCulture),
                DateTime.Parse(parts[4], CultureInfo.InvariantCulture)
            );
        }

        public TelemetryDataDto? SpanProcessTelemetry(string csvLine)
        {
            ReadOnlySpan<char> span = csvLine.AsSpan();
            int firstSemi = span.IndexOf(";");
            if (firstSemi == -1) return null;

            ReadOnlyMemory<char> vehicleMemory = csvLine.AsMemory(0, firstSemi);

            ReadOnlySpan<char> remaining = span.Slice(firstSemi + 1);
            int secondSemi = remaining.IndexOf(";");
            if (secondSemi == -1) return null;
            ReadOnlySpan<char> latSpan = remaining.Slice(0, secondSemi);

            remaining = remaining.Slice(secondSemi + 1);
            int thirdSemi = remaining.IndexOf(";");
            if (thirdSemi == -1) return null;
            ReadOnlySpan<char> lonSpan = remaining.Slice(0, thirdSemi);

            remaining = remaining.Slice(thirdSemi + 1);
            int fourthSemi = remaining.IndexOf(";");
            if (fourthSemi == -1) return null;
            ReadOnlySpan<char> speedSpan = remaining.Slice(0, fourthSemi);

            ReadOnlySpan<char> dateSpan = remaining.Slice(fourthSemi + 1);

            return new TelemetryDataDto
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
