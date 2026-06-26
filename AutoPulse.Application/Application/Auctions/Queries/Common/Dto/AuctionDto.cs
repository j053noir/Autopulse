using AutoPulse.Domain.Entities;

namespace AutoPulse.Application.Application.Auctions.Queries.Common.Dto
{
    public record AuctionDto(
        Guid? Id,
        VehicleDto? Vehicle,
        decimal? StartingPrice,
        string? StartingPriceCurrency,
        decimal? CurrentPrice,
        string? CurrentPriceCurrency,
        DateTime? EndTime,
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
