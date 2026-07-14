using AutoPulse.Notifications.Ingestion.Interfaces;
using AutoPulse.Notifications.Shared.Constants;
using AutoPulse.Notifications.Shared.Contracts;
using AutoPulse.Notifications.Shared.Serialization;
using Confluent.Kafka;
using System.Text.Json;

namespace AutoPulse.Notifications.Ingestion.Infrastructure
{
    public class KafkaNotificationPublisher : INotificationPublisher
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaNotificationPublisher> _logger;

        public KafkaNotificationPublisher
        (
            IConfiguration configuration,
            ILogger<KafkaNotificationPublisher> logger
        )
        {
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                LingerMs = 5,
                BatchSize = 64 * 1024,
                CompressionType = CompressionType.Snappy,
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync(NotificationEventPayload notificationPayload, CancellationToken cancellationToken = default)
        {
            string topic = DetermineTopicType(notificationPayload.Priority, notificationPayload.Channel);

            string messagePayload = JsonSerializer.Serialize(notificationPayload, AppJsonContext.Default.NotificationEventPayload);

            var message = new Message<string, string>
            {
                Key = notificationPayload.NotificationId.ToString(),
                Value = messagePayload
            };

            try
            {
                await _producer.ProduceAsync(topic, message, cancellationToken);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, $"Error publishing to Kafka for user {notificationPayload.UserId} in the topic {topic}");
                throw;
            }
        }

        private static string DetermineTopicType(string priority, string channel)
        {
            string sanitizedPriority = priority.ToLowerInvariant();
            string sanitizedChannel = channel.ToLowerInvariant();

            switch (sanitizedPriority)
            {
                case "transactional":
                    return GetTransactionalTopic(sanitizedChannel);
                case "marketing":
                    return NotificationTopic.MarketingBulk;
                default:
                    throw new ArgumentException($"Unknown priority: {priority}");
            }
        }

        private static string GetTransactionalTopic(string channel)
        {
            return channel switch
            {
                "email" => NotificationTopic.TransactionalEmail,
                "sms" => NotificationTopic.TransactionalSms,
                "push" => NotificationTopic.TransactionalPush,
                _ => throw new ArgumentException($"Unknown channel: {channel}")
            };
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(5));
            _producer.Dispose();
        }
    }
}
