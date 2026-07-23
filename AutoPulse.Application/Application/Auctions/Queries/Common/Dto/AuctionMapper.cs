using AutoPulse.Domain.Entities;

namespace AutoPulse.Application.Application.Auctions.Queries.Common.Dto
{
    internal static class AuctionMapper
    {
        /// <summary>
        /// Returns a placeholder AuctionDto with default values for all properties.
        /// </summary>
        /// <returns></returns>
        public static AuctionDto NullPlaceholder()
        {
            return new AuctionDto(
                Guid.Empty, // Id
                null, // Auctioneer
                null, // Vehicle
                decimal.Zero, // StartingPrice
                string.Empty, // StartingPriceCurrency
                decimal.Zero, // CurrentPrice
                string.Empty, // CurrentPriceCurrency
                DateTime.MinValue, // EndTime
                false, // IsActive
                null // Bids
            );
        }

        public static AuctionDto Map(Auction auction)
        {
            var AuctioneerDto = auction.Auctioneer != null
                ? new AuctioneerDto(
                    auction.Auctioneer.Id,
                    auction.Auctioneer.UserName,
                    auction.Auctioneer.Email,
                    auction.Auctioneer.IsActive
                )
                : null;
            var vehicleDto = auction.Vehicle != null
                ? new VehicleDto(
                    auction.Vehicle.Id,
                    auction.Vehicle.VIN,
                    auction.Vehicle.Marquee,
                    auction.Vehicle.Model,
                    auction.Vehicle.Year,
                    auction.Vehicle.Mileage,
                    auction.Vehicle.Title,
                    auction.Vehicle.BasePrice?.Amount,
                    auction.Vehicle.MinimumBidIncrement?.Amount,
                    auction.Vehicle.Category,
                    auction.Vehicle.DocumentStorageKey
                )
                : null;
            var bidDtos = auction.Bids?.Select(bid => new BidDto(
                bid.Id,
                bid.BidderId,
                bid.Bidder?.UserName,
                bid.Amount?.Amount,
                bid.Amount?.CurrencyCode,
                bid.CreatedAt.DateTime
            )).ToList();
            return new AuctionDto(
                auction.Id,
                AuctioneerDto,
                vehicleDto,
                auction.StartingPrice?.Amount,
                auction.StartingPrice?.CurrencyCode,
                auction.CurrentPrice?.Amount,
                auction.CurrentPrice?.CurrencyCode,
                auction.EndTime,
                auction.IsActive,
                bidDtos
            );
        }

        public static List<AuctionDto> MapList(IEnumerable<Auction> auctions)
        {
            var auctionDtos = new List<AuctionDto>();
            foreach (var auction in auctions)
            {
                auctionDtos.Add(Map(auction));
            }
            return auctionDtos;
        }
    }
}
