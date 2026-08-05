using System.Security.Claims;
using System.Text.Encodings.Web;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoPulse.Tests.Integration.Helpers;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";
    public static readonly Guid DefaultUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public const string DefaultFamilyId = "test-family-id";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, DefaultUserId.ToString()),
            new Claim("sub", DefaultUserId.ToString()),
            new Claim(Permissions.Claims.FamilyId, DefaultFamilyId),
            new Claim("UserId", DefaultUserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class TestPermissionCacheService : IPermissionCacheService
{
    public Task<HashSet<string>?> GetPermissionsAsync(Guid userId, string familyId, CancellationToken cancellationToken = default)
    {
        var permissions = new HashSet<string>
        {
            Permissions.Auctions.Read,
            Permissions.Auctions.Create,
            Permissions.Auctions.Bid,
            Permissions.Auctions.ReadBids,
            Permissions.Telemetry.Process,
            Permissions.Telemetry.Benchmark
        };

        return Task.FromResult<HashSet<string>?>(permissions);
    }

    public Task ServicePermissionsAsync(Guid userId, string familyId, HashSet<string> permissions, TimeSpan ttl, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RevokeUserAsync(Guid userId, string familyId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task InvalidateAllUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
