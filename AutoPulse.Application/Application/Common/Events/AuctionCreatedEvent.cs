using MediatR;

namespace AutoPulse.Application.Application.Common.Events
{
    public record AuctionCreatedEvent(
        Guid AuctionId,
        string Title,
        decimal BasePrice,
        DateTime EndTime,
        Guid AuctioneerId
    ) : INotification;
}
