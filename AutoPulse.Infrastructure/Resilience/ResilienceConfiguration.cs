using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Threading.RateLimiting;

namespace AutoPulse.Infrastructure.Resilience
{
    public static class ResilienceConfiguration
    {
        /// <summary>
        /// Payment policy gateway
        /// </summary>
        public const string GatewayPaymentPolicy = "GatewayPaymentPolicy";

        public static IServiceCollection AddResiliencePipelines(this IServiceCollection services)
        {
            services.AddResiliencePipeline(GatewayPaymentPolicy, (builder, context) =>
            {
                var logger = context.ServiceProvider.GetRequiredService<ILogger<ResiliencePipeline>>();

                builder
                    // 1.
                    .AddConcurrencyLimiter(new ConcurrencyLimiterOptions
                    {
                        PermitLimit = 10, // Max 10 concurrent threads processing payments
                        QueueLimit = 20, // Max 20 request in queue before the Fail-Fast
                    })
                    // 2. Circuit breaker layer
                    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                    {
                        FailureRatio = 0.5, // 50% failure ratio
                        SamplingDuration = TimeSpan.FromSeconds(10),
                        MinimumThroughput = 4,
                        BreakDuration = TimeSpan.FromSeconds(30),
                        OnOpened = args =>
                        {
                            logger.LogCritical("[Circuit Breaker] Circuit opened! Blocking traffic.");
                            return ValueTask.CompletedTask;
                        },
                        OnClosed = args =>
                        {
                            logger.LogInformation("[Circuit Breaker] Circuit closed. Service restablished.");
                            return ValueTask.CompletedTask;
                        },
                        OnHalfOpened = args =>
                        {
                            logger.LogWarning("[Circuit Breaker] Circuit half open. Testing traffic.");
                            return ValueTask.CompletedTask;
                        }
                    })
                    // 3. Retry Layer
                    .AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 3,
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                        Delay = TimeSpan.FromSeconds(2),
                        OnRetry = args =>
                        {
                            logger.LogWarning($"[Retry] Failed Attempt #{args.AttemptNumber}. Waiting {args.RetryDelay}. Exception: {args.Outcome.Exception?.Message}");
                            return ValueTask.CompletedTask;
                        }
                    });
            });

            return services;
        }
    }
}
