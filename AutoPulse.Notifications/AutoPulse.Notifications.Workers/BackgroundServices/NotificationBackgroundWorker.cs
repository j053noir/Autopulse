using AutoPulse.Notifications.Shared.Constants;
using AutoPulse.Notifications.Shared.Serialization;
using AutoPulse.Notifications.Workers.Interfaces;
using Confluent.Kafka;
using System.Text.Json;

namespace AutoPulse.Notifications.Workers.BackgroundServices
{
    public class NotificationBackgroundWorker : BackgroundService
    {
        private readonly ILogger<NotificationBackgroundWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public NotificationBackgroundWorker
        (
            ILogger<NotificationBackgroundWorker> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider
        )
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken));
        }

        private async Task StartConsumerLoop(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = KafkaConfig.NotificationWorkerGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(TopicsToSubscribe());

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    if (result is null) return;

                    _logger.LogInformation($"Message received in the partition {result.Partition} in the topic {result.Topic}");

                    var payload = JsonSerializer.Deserialize(result.Message.Value, AppJsonContext.Default.NotificationEventPayload);

                    if (payload is not null)
                    {
                        _logger.LogInformation($"Dispatching notification {payload.NotificationId} to channel {payload.Channel}");

                        using var scope = _serviceProvider.CreateScope();
                        var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();

                        await dispatcher.DispatchAsync(payload, stoppingToken);
                    }

                    consumer.Commit(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Kafka flow");
                }
            }
        }

        private static string[] TopicsToSubscribe()
        {
            return [
                NotificationTopic.TransactionalEmail,
                NotificationTopic.TransactionalSms,
                NotificationTopic.TransactionalPush,
                NotificationTopic.MarketingBulk,
            ];
        }
    }
}
