using AutoPulse.Domain.Entities;

namespace AutoPulse.Application.Application.Auctions.Queries.Common.Dto
{
    internal static class AuctionMapper
    {
        public static AuctionDto Map(Auction auction)
        {
            var vehicleDto = auction.Vehicle != null
                ? new VehicleDto(
                    auction.Vehicle.Id,
                    auction.Vehicle.VIN,
                    auction.Vehicle.Marquee,
                    auction.Vehicle.Model,
                    auction.Vehicle.Year,
                    auction.Vehicle.Mileage
                )
                : null;
            return new AuctionDto(
                auction.Id,
                vehicleDto,
                auction.StartingPrice?.Amount,
                auction.StartingPrice?.CurrencyCode,
                auction.CurrentPrice?.Amount,
                auction.CurrentPrice?.CurrencyCode,
                auction.EndTime,
                auction.IsActive
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
