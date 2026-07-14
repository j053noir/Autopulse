using AutoPulse.Notifications.Shared.Contracts;
using System.Text.Json.Serialization;

namespace AutoPulse.Notifications.Shared.Serialization
{
    [JsonSerializable(typeof(IngestNotificationRequest))]
    [JsonSerializable(typeof(NotificationEventPayload))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    public partial class AppJsonContext : JsonSerializerContext
    {
    }
}
