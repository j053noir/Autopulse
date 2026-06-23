namespace AutoPulse.Application.Application.Auctions.Queries.GetAuctionById
{
    public record AuctionDto(
        Guid Id,
        string VehicleModel,
        decimal CurrentPrice,
        DateTime EndTime,
        bool IsActive
    );
}
