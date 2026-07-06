using AutoPulse.Application.Application.Common.Interfaces;

namespace AutoPulse.Application.Application.Common.Events
{
    public record AuctionEndedIntegrationEvent(
        Guid EventId,
        Guid AuctionId,
        Guid WinnerId,
        decimal Amount,
        string Currency,
        DateTime OccuredOn
    ) : IIntegrationEvent;
}
