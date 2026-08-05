using System.Net;
using System.Net.Http.Json;
using AutoPulse.Application.Application.Authentication.Commands.LoginUser;
using AutoPulse.Application.Application.Authentication.Commands.RegisterUser;
using AutoPulse.Application.Application.Authentication.Common.Dto;
using AutoPulse.Domain.Common.Security;
using AutoPulse.Domain.Entities;
using AutoPulse.Infrastructure.Persistence.Sql;
using AutoPulse.Tests.Integration.Fixtures;
using AutoPulse.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AutoPulse.Tests.Integration.Endpoints;

public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(CustomWebApplicationFactory factory)
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
                "testuser",
                "testuser@autopulse.com",
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
    public async Task Register_WithValidPayload_Returns200Ok_AndPersistsUserInDatabase()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Username: "newdriver",
            Email: "driver@autopulse.com",
            Password: "SecurePassword123!",
            IdempotencyKey: Guid.NewGuid()
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultJson = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        resultJson.Should().NotBeNull();
        resultJson.Should().ContainKey("id");

        var newUserId = resultJson!["id"];
        newUserId.Should().NotBeEmpty();

        // Verify database persistence directly in PostgreSQL
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AutoPulseDbContext>();
        var userFromDb = await dbContext.Set<User>().FirstOrDefaultAsync(u => u.Id == newUserId);

        userFromDb.Should().NotBeNull();
        userFromDb!.Email.Should().Be("driver@autopulse.com");
        userFromDb.UserName.Should().Be("newdriver");
    }

    [Fact]
    public async Task Login_WithRegisteredUser_Returns200Ok_AndSetsCookies()
    {
        // Arrange - Register user first
        var email = "buyer@autopulse.com";
        var password = "BuyerPassword123!";
        var registerCommand = new RegisterUserCommand(
            Username: "buyer",
            Email: email,
            Password: password,
            IdempotencyKey: Guid.NewGuid()
        );

        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
        regResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Attempt login
        var loginCommand = new LoginUserCommand(
            Email: email,
            Password: password
        );

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var authDto = await loginResponse.Content.ReadFromJsonAsync<AuthDto>();
        authDto.Should().NotBeNull();
        authDto!.AccessToken.Should().NotBeNullOrEmpty();
        authDto.RefreshToken.Should().NotBeNullOrEmpty();

        // Verify Set-Cookie headers
        loginResponse.Headers.Should().ContainKey("Set-Cookie");
        var cookieHeaders = loginResponse.Headers.GetValues("Set-Cookie");
        cookieHeaders.Should().Contain(c => c.Contains("autopulse-session"));
        cookieHeaders.Should().Contain(c => c.Contains("autopulse-refresh-token"));
    }

    [Fact]
    public async Task GetProfile_AuthenticatedUser_Returns200Ok_AndUserProfileData()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();

        profile.Should().NotBeNull();
        profile!.Id.Should().Be(TestAuthHandler.DefaultUserId);
    }
}
