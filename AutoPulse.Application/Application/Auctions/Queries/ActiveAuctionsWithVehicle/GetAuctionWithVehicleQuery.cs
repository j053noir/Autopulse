using AutoPulse.Application.Application.Auctions.Queries.Common.Dto;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Queries.ActiveAuctionsWithVehicle
{
    /// <summary>
    /// Query elements used to retrieve and filter a list of available auctions.
    /// </summary>
    /// <param name="Marquee">Filters auctions by the specific vehicle manufacturer (e.g., 'Tesla', 'Toyota'). Case-insensitive.</param>
    /// <param name="MinYear">Filters auctions where the vehicle manufacture year is greater than or equal to this value.</param>
    /// <param name="MaxPrice">Filters auctions where the current highest bid or starting price is less than or equal to this amount.</param>
    public record GetAuctionWithVehicleQuery
        (
            string? Marquee,
            int? MinYear,
            decimal? MaxPrice
        ) : IRequest<IReadOnlyList<AuctionDto?>>;
}
