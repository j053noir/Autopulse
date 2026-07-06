using AutoPulse.Application.Application.Auctions.Queries.Common.Dto;
using AutoPulse.Application.Application.Common.Interfaces;

namespace AutoPulse.Application.Application.Auctions.Queries.ActiveAuctionsWithVehicle
{
    /// <summary>
    /// Query elements used to retrieve and filter a list of available auctions.
    /// </summary>
    /// <param name="AuctionId">Filters auctions by the specific auction ID.</param>
    /// <param name="AuctioneerId">Filters auctions by the specific auctioneer ID.</param>
    /// <param name="Marquee">Filters auctions by the specific vehicle manufacturer (e.g., 'Tesla', 'Toyota'). Case-insensitive.</param>
    /// <param name="MinYear">Filters auctions where the vehicle manufacture year is greater than or equal to this value.</param>
    /// <param name="MaxPrice">Filters auctions where the current highest bid or starting price is less than or equal to this amount.</param>
    public record GetAuctionWithVehicleQuery
        (
            Guid AuctionId,
            Guid AuctioneerId,
            string? Marquee,
            int? MinYear,
            decimal? MaxPrice
        ) : IReadOnlyQuery<IReadOnlyList<AuctionDto?>>;
}
