using AutoPulse.Notifications.Workers.Providers;
using Polly;
using Polly.Wrap;

namespace AutoPulse.Notifications.Workers.Infrastructure.Providers.Sms
{
    public class ResilientSmsProvider : ISmsProvider
    {
        public string ProviderName => "ResilientSmsGateway";

        private readonly ISmsProvider _primaryProvider;
        private readonly ISmsProvider _fallbackProvider;
        private readonly ILogger<ResilientSmsProvider> _logger;
        private readonly AsyncPolicyWrap _resiliencePolicy;

        public ResilientSmsProvider
        (
            ISmsProvider primaryProvider,
            ISmsProvider fallbackProvider,
            ILogger<ResilientSmsProvider> logger
        )
        {


            _primaryProvider = primaryProvider;
            _fallbackProvider = fallbackProvider;
            _logger = logger;

            // 1. Retry Policy
            var retryPolicy = Policy.Handle<Exception>()
                                    .WaitAndRetryAsync(3, retryAttempt =>
                                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                        (exception, delay, retryCount, context) =>
                                        {
                                            _logger.LogWarning(
                                                $"Attemp {retryCount} failed, " +
                                                $"Trying again in {delay.Milliseconds}ms. " +
                                                $"Error: {exception.Message}"
                                            );
                                        });

            // 2. Fallback Policy
            var fallbackPolicy = Policy.Handle<Exception>()
                                        .FallbackAsync
                                        (
                                            fallbackAction: async (context, cancellationToken) =>
                                            {
                                                _logger.LogCritical($"primary Provider unnaccesible. " +
                                                    $"Communiting to fallback provider: {_fallbackProvider.ProviderName}");

                                                string? to = (string)ContextExtensions.GetContextData(context, ContextKeys.To);
                                                string? message = (string)ContextExtensions.GetContextData(context, ContextKeys.Message);

                                                await _fallbackProvider.SendSmsAsync(to!, message!, cancellationToken);
                                            },
                                            onFallbackAsync: async (exception, context) =>
                                            {
                                                _logger.LogWarning($"Initializing conmutation by error (Fallback): {exception.Message}");
                                                await Task.CompletedTask;
                                            }
                                        );


            _resiliencePolicy = Policy.WrapAsync(fallbackPolicy, retryPolicy);
        }

        public async Task SendSmsAsync(string to, string message, CancellationToken cancellationToken)
        {
            var context = new Context
            {
                [ContextKeys.To] = to,
                [ContextKeys.Message] = message
            };

            await _resiliencePolicy.ExecuteAsync
            (
                async (ctx, tkn) => await _primaryProvider.SendSmsAsync(to, message, tkn),
                context,
                cancellationToken
            );
        }
    }

    internal static class ContextKeys
    {
        public const string To = "to";
        public const string Message = "message";
    }

    internal static class ContextExtensions
    {
        public static object GetContextData(Context context, string key)
        {
            return context.TryGetValue(key, out var value) ? value : null!;
        }
    }
}
