using AutoPulse.Application.Application.Common.Interfaces;

namespace AutoPulse.Application.Application.Common.Messaging.Events
{
    public record PaymentFailedEvent(
        Guid EventId,
        Guid TransactionId,
        string ErrorCode,
        string ErrorMessage,
        DateTime OccuredOn
    ) : IIntegrationEvent;
}
