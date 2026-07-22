namespace AutoPulse.Domain.Events
{
    public record AuctionCreatedDomainEvent(
        Guid AuctionId,
        string Title,
        decimal BasePrice,
        DateTime EndTime,
        Guid AuctioneerId
    );
}
