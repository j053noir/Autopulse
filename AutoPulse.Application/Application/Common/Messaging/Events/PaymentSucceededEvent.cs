using AutoPulse.Application.Application.Common.Interfaces;

namespace AutoPulse.Application.Application.Common.Messaging.Events
{
    public record PaymentSucceededEvent(
        Guid EventId,
        Guid TransactionId,
        string TransactionReference,
        decimal AmountProcessed,
        string Currency,
        string PaymentMethod,
        DateTime OccuredOn
    ) : IIntegrationEvent;
}
