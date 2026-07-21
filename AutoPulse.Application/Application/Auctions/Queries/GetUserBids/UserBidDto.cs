using System;

namespace AutoPulse.Application.Application.Auctions.Queries.GetUserBids
{
    public record UserBidDto(
        Guid BidId,
        Guid AuctionId,
        string VehicleName,
        decimal BidAmount,
        string Currency,
        DateTime BidDate,
        decimal CurrentAuctionPrice,
        bool IsAuctionActive
    );
}
