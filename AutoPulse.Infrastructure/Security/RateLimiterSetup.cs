using AutoPulse.Domain.Common.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Threading.RateLimiting;

namespace AutoPulse.Infrastructure.Security
{
    public static class RateLimiterSetup
    {
        public static IServiceCollection AddDistributedRateLimiter(this IServiceCollection services, IConfiguration configuration)
        {
            var cacheConnectionString = configuration.GetConnectionString("CacheConnection") ??
                    throw new InvalidOperationException("Cache connection string is missing");

            var cacheConnection = ConnectionMultiplexer.Connect(cacheConnectionString);
            services.AddSingleton(cacheConnection);

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too Many Requests",
                        Detail = "Gloabl reqeuest limit exceeded. Try again in some minutes later..."
                    }, cancellationToken);
                };

                options.AddPolicy(Permissions.Policies.AuthPolicy, context =>
                {
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "uknowkn";
                    var cachePartitionKey = $"{Permissions.CacheKeys.RateLimitAuth}:{clientIp}";

                    return RateLimitPartition.GetFixedWindowLimiter(cachePartitionKey, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0,
                    });
                });

                options.AddPolicy(Permissions.Policies.ApiGeneralPolicy, context =>
                {
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "uknowkn";
                    var cachePartitionKey = $"{Permissions.CacheKeys.RateLimitGeneral}:{clientIp}";

                    return RateLimitPartition.GetTokenBucketLimiter(cachePartitionKey, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 60,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(60),
                        TokensPerPeriod = 10,
                        QueueLimit = 2,
                    });
                });
            });

            return services;
        }
    }
}
