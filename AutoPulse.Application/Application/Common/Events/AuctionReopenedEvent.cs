using AutoPulse.Application.Application.Common.Interfaces;

namespace AutoPulse.Application.Application.Common.Events
{
    public record AuctionReopenedEvent(
        Guid EventId,
        Guid TransactionId,
        Guid AuctionId,
        DateTime OccuredOn
    ) : IIntegrationEvent;
}
