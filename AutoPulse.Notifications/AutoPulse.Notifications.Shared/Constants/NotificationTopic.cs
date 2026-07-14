namespace AutoPulse.Notifications.Shared.Constants
{
    public static class NotificationTopic
    {
        // Telemetry
        public const string TelemetryEvents = "notification.telemetry.events";

        // Channels
        public const string TransactionalEmail = "notification.transactional.email";
        public const string TransactionalSms = "notification.transactional.sms";
        public const string TransactionalPush = "notification.transactional.push";

        // Marketing
        public const string MarketingBulk = "notification.marketing.bulk";
    }
}
