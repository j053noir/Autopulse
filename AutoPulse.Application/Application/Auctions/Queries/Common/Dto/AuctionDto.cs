namespace AutoPulse.Application.Application.Auctions.Queries.Common.Dto
{
    /// <summary>
    /// Represents a data transfer object (DTO) for an auction
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Auctioneer"></param>
    /// <param name="Vehicle"></param>
    /// <param name="StartingPrice"></param>
    /// <param name="StartingPriceCurrency"></param>
    /// <param name="CurrentPrice"></param>
    /// <param name="CurrentPriceCurrency"></param>
    /// <param name="EndTime"></param>
    /// <param name="IsActive"></param>
    /// <param name="Bids"></param>
    public record AuctionDto(
        Guid? Id,
        AuctioneerDto? Auctioneer,
        VehicleDto? Vehicle,
        decimal? StartingPrice,
        string? StartingPriceCurrency,
        decimal? CurrentPrice,
        string? CurrentPriceCurrency,
        DateTime? EndTime,
        bool? IsActive,
        ICollection<BidDto>? Bids
    );

    /// <summary>
    /// Represents a data transfer object (DTO) for an auctioneer
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="UserName"></param>
    /// <param name="Email"></param>
    /// <param name="IsActive"></param>
    public record AuctioneerDto(
        Guid? Id,
        string? UserName,
        string? Email,
        bool? IsActive
    );

    /// <summary>
    /// Represents a data transfer object (DTO) for a vehicle
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="VIN"></param>
    /// <param name="Marquee"></param>
    /// <param name="Model"></param>
    /// <param name="Year"></param>
    /// <param name="Mileage"></param>
    public record VehicleDto(
        Guid? Id,
        string? VIN,
        string? Marquee,
        string? Model,
        int? Year,
        int? Mileage
    );

    /// <summary>
    /// Represents a data transfer object (DTO) for a bid
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="BidderId"></param>
    /// <param name="Amount"></param>
    /// <param name="Currency"></param>
    /// <param name="CreatedAt"></param>
    public record BidDto(
        Guid? Id,
        Guid? BidderId,
        decimal? Amount,
        string? Currency,
        DateTime? CreatedAt
    );
}
