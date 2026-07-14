using AutoPulse.Notifications.Workers.Providers;
using Polly;
using Polly.Wrap;

namespace AutoPulse.Notifications.Workers.Infrastructure.Providers.Email
{
    public class ResilientEmailProvider : IEmailProvider
    {
        public string ProviderName => "ResilientEmailGateway";

        private readonly IEmailProvider _primaryProvider;
        private readonly IEmailProvider _fallbackProvider;
        private readonly ILogger<ResilientEmailProvider> _logger;
        private readonly AsyncPolicyWrap _resiliencePolicy;

        public ResilientEmailProvider
        (
            IEmailProvider primaryProvider,
            IEmailProvider fallbackProvider,
            ILogger<ResilientEmailProvider> logger
        )
        {
            _primaryProvider = primaryProvider;
            _fallbackProvider = fallbackProvider;
            _logger = logger;

            var retryPolicy = Policy.Handle<Exception>()
                                        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                            (ex, time) => _logger.LogWarning($"Email sent failed. Retrying in {time.TotalSeconds}"));

            var fallbackPolicy = Policy.Handle<Exception>()
                                        .FallbackAsync(
                                            fallbackAction: async (context, cancellationToken) =>
                                            {
                                                _logger.LogCritical($"primary Provider unnaccesible. " +
                                                    $"Communiting to fallback provider: {_fallbackProvider.ProviderName}");

                                                string? to = (string)ContextExtensions.GetContextData(context, ContextKeys.To);
                                                string? subject = (string)ContextExtensions.GetContextData(context, ContextKeys.Subject);
                                                string body = (string)ContextExtensions.GetContextData(context, ContextKeys.Body);

                                                await _fallbackProvider.SendEmailAsync(to, subject, body, cancellationToken);
                                            },
                                            onFallbackAsync: async (exception, context) =>
                                            {
                                                _logger.LogWarning($"Initializing conmutation by error (Fallback): {exception.Message}");
                                                await Task.CompletedTask;
                                            });

            _resiliencePolicy = Policy.WrapAsync(fallbackPolicy, retryPolicy);
        }

        public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            var context = new Context
            {
                [ContextKeys.To] = to,
                [ContextKeys.Subject] = subject,
                [ContextKeys.Body] = body,
            };

            await _resiliencePolicy.ExecuteAsync(
                    async (ctx, token) => await _primaryProvider.SendEmailAsync(to, subject, body, token),
                    context,
                    cancellationToken
                );
        }
    }

    internal static class ContextKeys
    {
        public const string To = "to";
        public const string Subject = "subject";
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
