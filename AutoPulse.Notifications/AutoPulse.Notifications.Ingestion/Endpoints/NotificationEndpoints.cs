using AutoPulse.Notifications.Ingestion.Interfaces;
using AutoPulse.Notifications.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AutoPulse.Notifications.Ingestion.Endpoints
{
    public static class NotificationEndpoints
    {
        public static IEndpointRouteBuilder MapNotificationRoutes(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/notifications");

            group.MapPost("/", async
            (
                [FromBody] IngestNotificationRequest request,
                [FromServices] IPreferenceService preferenceService,
                INotificationPublisher publisher,
                CancellationToken cancellationToken = default
            ) =>
            {
                // 1. Rapid basic validation
                if (request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Target))
                {
                    return Results.BadRequest("Invalid destinatary data");
                }

                // 2. Short-circuit check
                bool isAllowed = await preferenceService.IsChannelAllowedAsync(request.UserId.ToString(), request.Channel, cancellationToken);
                if (!isAllowed)
                {
                    return Results.Accepted();
                }

                // 3. Map the domain event
                var payload = new NotificationEventPayload
                (
                    Guid.NewGuid(),
                    request.UserId,
                    request.Priority,
                    request.Channel,
                    request.Target,
                    request.TemplateId,
                    request.TemplateVariables,
                    DateTime.UtcNow
                );

                // 4. Enqueue in Kafka
                await publisher.PublishAsync(payload, cancellationToken);

                return Results.Accepted(uri: $"/api/v1/notifications/{payload.NotificationId}");
            });

            return app;
        }
    }
}
