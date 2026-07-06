using MediatR;

namespace AutoPulse.Application.Application.Common.Events
{
    public record AuctionEndedEvent(
        Guid AuctionId,
        Guid WinnerId,
        decimal FinalPrice,
        string Currency,
        DateTime OcurredOn
    ) : INotification;
}
