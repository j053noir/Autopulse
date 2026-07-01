namespace AutoPulse.Application.Application.Auctions.Queries.Common.Dto
{
    public record AuctionDto(
        Guid? Id,
        AuctioneerDto? Auctioneer,
        VehicleDto? Vehicle,
        decimal? StartingPrice,
        string? StartingPriceCurrency,
        decimal? CurrentPrice,
        string? CurrentPriceCurrency,
        DateTime? EndTime,
        bool? IsActive
    );

    public record AuctioneerDto(
        Guid? Id,
        string? UserName,
        string? Email,
        bool? IsActive
    );

    public record VehicleDto(
        Guid? Id,
        string? VIN,
        string? Marquee,
        string? Model,
        int? Year,
        int? Mileage
    );
}
