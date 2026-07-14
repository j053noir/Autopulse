using AutoPulse.Notifications.Workers.Providers;
using Polly;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoPulse.Notifications.Workers.Infrastructure.Providers.Push
{
    public class ResilientPushProvider : IPushProvider
    {
        public string ProviderName => "ResilientPushGateway";

        private readonly ILogger<ResilientPushProvider> _logger;
        private readonly IPushProvider _primaryProvider;
        private readonly IPushProvider _fallbackProvider;
        private readonly AsyncPolicyWrap _resiliencePolicy;

        public ResilientPushProvider
        (
            ILogger<ResilientPushProvider> logger,
            IPushProvider primaryProvider,
            IPushProvider fallbackProvider
        )
        {
            _logger = logger;
            _primaryProvider = primaryProvider;
            _fallbackProvider = fallbackProvider;

            var retryPolicy = Policy.Handle<Exception>()
                                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)));

            var fallbackPolicy = Policy.Handle<Exception>()
                                        .FallbackAsync(
                                        fallbackAction: async (context, cancellationToken) =>
                                        {
                                            _logger.LogCritical($"primary Provider unnaccesible. " +
                                                $"Communiting to fallback provider: {_fallbackProvider.ProviderName}");

                                            string? token = (string)ContextExtensions.GetContextData(context, ContextKeys.Token);
                                            string? title = (string)ContextExtensions.GetContextData(context, ContextKeys.Title);
                                            string body = (string)ContextExtensions.GetContextData(context, ContextKeys.Body);

                                            await _fallbackProvider.SendAsync(token, title, body, cancellationToken);
                                        },
                                        onFallbackAsync: async (exception, context) =>
                                        {
                                            _logger.LogWarning($"Initializing conmutation by error (Fallback): {exception.Message}");
                                            await Task.CompletedTask;
                                        });

            _resiliencePolicy = Policy.WrapAsync(fallbackPolicy, retryPolicy);
        }

        public async Task SendAsync(string deviceToken, string title, string body, CancellationToken cancellationToken)
        {
            var context = new Context
            {
                [ContextKeys.Token] = deviceToken,
                [ContextKeys.Title] = title,
                [ContextKeys.Body] = body,
            };

            await _resiliencePolicy.ExecuteAsync(
                    async (ct, token) => await _primaryProvider.SendAsync(deviceToken, title, body, token),
                    context,
                    cancellationToken
                );
        }
    }

    internal static class ContextKeys
    {
        public const string Token = "token";
        public const string Title = "title";
        public const string Body = "body";
    }

    internal static class ContextExtensions
    {
        public static object GetContextData(Context context, string key)
        {
            return context.TryGetValue(key, out var value) ? value : null!;
        }
    }
}
