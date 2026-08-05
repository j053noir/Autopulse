using System.Net;
using System.Net.Http.Json;
using AutoPulse.Application.Application.Auctions.Commands.CreateAuction;
using AutoPulse.Application.Application.Auctions.Commands.CreateAuctionBid;
using AutoPulse.Application.Application.Auctions.Queries.Common.Dto;
using AutoPulse.Application.Application.Auctions.Queries.GetAuctionDashboard.Dto;
using AutoPulse.Domain.Common.Security;
using AutoPulse.Domain.Entities;
using AutoPulse.Infrastructure.Persistence.Sql;
using AutoPulse.Tests.Integration.Fixtures;
using AutoPulse.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using AutoPulse.Application.Application.Auctions.Queries.GetUserBids;

namespace AutoPulse.Tests.Integration.Endpoints;


public class AuctionsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuctionsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AutoPulseDbContext>();

        if (!await dbContext.Set<User>().AnyAsync(u => u.Id == TestAuthHandler.DefaultUserId))
        {
            var user = User.Create(
                TestAuthHandler.DefaultUserId,
                "testauctioneer",
                "auctioneer@autopulse.com",
                "hashedpassword",
                [Permissions.Auctions.Read, Permissions.Auctions.Create, Permissions.Auctions.Bid, Permissions.Auctions.ReadBids]
            );
            dbContext.Set<User>().Add(user);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    [Fact]
    public async Task GetActiveAuctions_Returns200Ok_AndValidJsonListStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/auctions/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auctions = await response.Content.ReadFromJsonAsync<IReadOnlyList<AuctionDto>>();
        auctions.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAuction_WithValidPayload_Returns200Ok_AndPersistsInDatabase()
    {
        // Arrange
        var command = new CreateAuctionCommand(
            Vin: "1HGCR2F83HA000000",
            Marquee: "Honda",
            Model: "Accord",
            Year: 2022,
            Mileage: 15000,
            Title: "2022 Honda Accord EX-L",
            BasePrice: 25000m,
            MinimumBidIncrement: 500m,
            Category: "Sedan",
            DocumentStorageKey: "docs/honda-accord-title.pdf",
            AuctioneerId: TestAuthHandler.DefaultUserId,
            StartingPrice: 25000m,
            Currency: "USD",
            EndTime: DateTime.UtcNow.AddDays(7),
            IdempotencyKey: Guid.NewGuid()
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auctions", command);

        // Assert HTTP response
        if (!response.IsSuccessStatusCode)
        {
            var errContent = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.Created], $"Response failed with content: {errContent}");
        }

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        var resultJson = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        resultJson.Should().NotBeNull();
        resultJson.Should().ContainKey("id");

        var createdAuctionId = resultJson!["id"];
        createdAuctionId.Should().NotBeEmpty();

        // Validate database persistence directly in PostgreSQL
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AutoPulseDbContext>();
        var auctionFromDb = await dbContext.Auctions.Include(a => a.Vehicle).FirstOrDefaultAsync(a => a.Id == createdAuctionId);

        auctionFromDb.Should().NotBeNull();
        auctionFromDb!.Vehicle.Should().NotBeNull();
        auctionFromDb.Vehicle!.Title.Should().Be("2022 Honda Accord EX-L");
        auctionFromDb.StartingPrice!.Amount.Should().Be(25000m);

        // Also validate query endpoint GET /api/auctions/{id}
        var getByIdResponse = await _client.GetAsync($"/api/auctions/{createdAuctionId}");
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var auctionDto = await getByIdResponse.Content.ReadFromJsonAsync<AuctionDto>();
        auctionDto.Should().NotBeNull();
        auctionDto!.Id.Should().Be(createdAuctionId);
        auctionDto.Vehicle.Should().NotBeNull();
        auctionDto.Vehicle!.Title.Should().Be("2022 Honda Accord EX-L");
    }

    [Fact]
    public async Task PlaceBidOnAuction_WithValidAmount_Returns200Ok_AndUpdatesCurrentPrice()
    {
        // 1. Create Auction first
        var createCommand = new CreateAuctionCommand(
            Vin: "1HGCR2F83HA000000",
            Marquee: "Toyota",
            Model: "Camry",
            Year: 2023,
            Mileage: 10000,
            Title: "2023 Toyota Camry SE",
            BasePrice: 20000m,
            MinimumBidIncrement: 500m,
            Category: "Sedan",
            DocumentStorageKey: "docs/toyota-title.pdf",
            AuctioneerId: TestAuthHandler.DefaultUserId,
            StartingPrice: 20000m,
            Currency: "USD",
            EndTime: DateTime.UtcNow.AddDays(5),
            IdempotencyKey: Guid.NewGuid()
        );

        var createRes = await _client.PostAsJsonAsync("/api/auctions", createCommand);
        createRes.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        var auctionResult = await createRes.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var auctionId = auctionResult!["id"];

        // 2. Place bid on auction
        var bidCommand = new CreateAuctionBidCommand(
            AuctionId: auctionId,
            BidderId: TestAuthHandler.DefaultUserId,
            Amount: 21000m,
            Currency: "USD",
            IdempotencyKey: Guid.NewGuid()
        );

        var bidResponse = await _client.PostAsJsonAsync($"/api/auctions/{auctionId}/bids", bidCommand);
        bidResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var bidResult = await bidResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        bidResult.Should().NotBeNull();
        bidResult.Should().ContainKey("id");

        // 3. Verify Dashboard endpoint
        var dashboardRes = await _client.GetAsync($"/api/auctions/{auctionId}/dashboard");
        dashboardRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var dashboardDto = await dashboardRes.Content.ReadFromJsonAsync<AuctionDashboardDto>();
        dashboardDto.Should().NotBeNull();
        dashboardDto!.HistoricBids.Should().HaveCount(1);
        dashboardDto.HistoricBids[0].Amount.Should().Be(21000m);
    }


    [Fact]
    public async Task GetMyBids_Returns200Ok_AndUserBidsList()
    {
        // Act
        var response = await _client.GetAsync("/api/auctions/bids/my");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bids = await response.Content.ReadFromJsonAsync<IReadOnlyList<UserBidDto>>();
        bids.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAuction_WithInvalidVin_Returns400BadRequest()
    {
        // Arrange - Invalid VIN (less than 17 characters)
        var invalidCommand = new CreateAuctionCommand(
            Vin: "INVALID_VIN",
            Marquee: "Honda",
            Model: "Accord",
            Year: 2022,
            Mileage: 15000,
            Title: "2022 Honda Accord EX-L",
            BasePrice: 25000m,
            MinimumBidIncrement: 500m,
            Category: "Sedan",
            DocumentStorageKey: "docs/honda-accord-title.pdf",
            AuctioneerId: TestAuthHandler.DefaultUserId,
            StartingPrice: 25000m,
            Currency: "USD",
            EndTime: DateTime.UtcNow.AddDays(7),
            IdempotencyKey: Guid.NewGuid()
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auctions", invalidCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("VIN");
    }
}
