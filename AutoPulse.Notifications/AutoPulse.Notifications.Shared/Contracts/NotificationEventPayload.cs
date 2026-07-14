namespace AutoPulse.Notifications.Shared.Contracts
{
    /// <summary>
    /// Defines the structure of a notification event payload.
    /// </summary>
    /// <param name="NotificationId">The unique identifier of the notification.</param>
    /// <param name="UserId">The unique identifier of the user.</param>
    /// <param name="Priority">The priority of the notification.</param>
    /// <param name="Channel">The channel through which the notification is sent.</param>
    /// <param name="Target">The target of the notification. (User's phone, email, or push token)</param>
    /// <param name="TemplateId">The identifier of the template to use for the notification.</param>
    /// <param name="TemplateVariables">The variables to use for the template.</param>
    /// <param name="CreatedAt">The time at which the notification was created.</param>
    public record NotificationEventPayload
    (
        Guid NotificationId,
        Guid UserId,
        string Priority,
        string Channel,
        string Target,
        string TemplateId,
        Dictionary<string, string> TemplateVariables,
        DateTime CreatedAt
    );
}
