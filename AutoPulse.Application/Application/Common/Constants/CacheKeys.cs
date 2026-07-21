using System;

namespace AutoPulse.Application.Application.Common.Constants
{
    public static class CacheKeys
    {
        // Auctions
        public const string ActiveAuctionsList = "auctions:list_active";
        
        public static string AuctionDetail(Guid auctionId) => $"auctions:detail:{auctionId}";

        public static string ActiveAuctionsFiltered(Guid? auctionId, Guid? auctioneerId, string? marquee, int? minYear, decimal? maxPrice)
        {
            if (auctionId == Guid.Empty &&
                auctioneerId == Guid.Empty &&
                string.IsNullOrEmpty(marquee) &&
                !minYear.HasValue && !maxPrice.HasValue)
            {
                return ActiveAuctionsList;
            }

            var auctionIdPart = auctionId.HasValue ? auctionId.Value.ToString() : "null";
            var auctioneerIdPart = auctioneerId.HasValue ? auctioneerId.Value.ToString() : "null";
            var marqueePart = !string.IsNullOrEmpty(marquee) ? marquee : "null";
            var minYearPart = minYear.HasValue ? minYear.Value.ToString() : "null";
            var maxPricePart = maxPrice.HasValue ? maxPrice.Value.ToString() : "null";

            return $"auctions:list_active:{auctionIdPart}:{auctioneerIdPart}:{marqueePart}:{minYearPart}:{maxPricePart}";
        }

        // Vehicles
        public static string VehicleSpecs(Guid auctionId) => $"vehicles:specs:{auctionId}";

        // Users
        public static string UserProfile(Guid userId) => $"users:profile:{userId}";
        public static string UserBids(Guid userId) => $"auctions:user-bids:{userId}";
    }
}
