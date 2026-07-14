namespace AutoPulse.Notifications.Shared.Contracts
{
    /// <summary>
    /// Defines the structure of a request to ingest a notification.
    /// </summary>
    /// <param name="UserId">The unique identifier of the user.</param>
    /// <param name="Priority">The priority of the notification. Valid values are "transactional" and "marketing".</param>
    /// <param name="Channel">The channel through which the notification is sent. Valid values are "sms", "email", and "push".</param>
    /// <param name="Target">The target of the notification. (User's phone, email, or push token)</param>
    /// <param name="TemplateId">The identifier of the template to use for the notification.</param>
    /// <param name="TemplateVariables">The variables to use for the template.</param>
    public record IngestNotificationRequest
    (
        Guid UserId,
        Guid AuctionId,
        string Priority,
        string Channel,
        string Target,
        string TemplateId,
        Dictionary<string, string> TemplateVariables
    );
}
