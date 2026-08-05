using System.Net;
using System.Net.Http.Json;
using AutoPulse.Tests.Integration.Fixtures;
using FluentAssertions;
using Xunit;

namespace AutoPulse.Tests.Integration.Endpoints;

public class TelemetryControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TelemetryControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("span")]
    [InlineData("naive")]
    public async Task ProcessTelemetry_WithValidRawData_Returns200Ok(string method)
    {
        // Arrange - CSV line format: VehicleId;Latitude;Longitude;Speed;Timestamp
        var rawTelemetryData = "VEH123;4.60971;-74.08175;80.5;2026-08-04T22:00:00Z";

        // Act
        var response = await _client.PostAsJsonAsync($"/api/telemetry?method={method}", rawTelemetryData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
