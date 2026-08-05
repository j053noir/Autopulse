using System.Data.Common;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Infrastructure.Persistence.Sql;
using AutoPulse.Tests.Integration.Helpers;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace AutoPulse.Tests.Integration.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("autopulse_test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Prevent Redis connection failure on Startup by setting abortConnect=false
        builder.UseSetting("ConnectionStrings:CacheConnection", "127.0.0.1:6379,abortConnect=false");

        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContextOptions descriptor if present
            services.RemoveAll<DbContextOptions<AutoPulseDbContext>>();

            // Register DbContext with Testcontainers connection string
            services.AddDbContext<AutoPulseDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
                options.UseExceptionProcessor();
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            });

            // Use Memory Cache instead of Redis for IDistributedCache in tests
            services.RemoveAll<IDistributedCache>();
            services.AddDistributedMemoryCache();

            // Replace permission cache service with test double
            services.RemoveAll<IPermissionCacheService>();
            services.AddSingleton<IPermissionCacheService, TestPermissionCacheService>();

            // Configure Test Authentication Handler as default scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    public async Task InitializeAsync()
    {
        // 1. Start ephemeral PostgreSQL container
        await _dbContainer.StartAsync();

        // 2. Apply EF Core migrations
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AutoPulseDbContext>();
        await dbContext.Database.MigrateAsync();

        // 3. Setup Respawn for fast database resets
        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner != null && _dbConnection != null)
        {
            await _respawner.ResetAsync(_dbConnection);
        }
    }

    public new async Task DisposeAsync()
    {
        if (_dbConnection != null)
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }

        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
