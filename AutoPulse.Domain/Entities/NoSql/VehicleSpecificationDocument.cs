using AutoPulse.Domain.Common;

namespace AutoPulse.Domain.Entities.NoSql
{
    public class VehicleSpecificationDocument : BaseDocument, IAggregateRoot
    {
        public string AuctionId { get; private set; }
        public string Model { get; private set; }
        public string Brand { get; private set; }

        public Dictionary<string, object> DynamicMetadata { get; private set; } = new();

        public static VehicleSpecificationDocument Create(
            string auctionId,
            string model,
            string brand,
            Dictionary<string, object> dynamicMetadata
        )
        {
            if (string.IsNullOrWhiteSpace(auctionId)) throw new ArgumentNullException("AuctionId is required");
            if (string.IsNullOrEmpty(model)) throw new ArgumentNullException("model is required");
            if (string.IsNullOrWhiteSpace(brand)) throw new ArgumentNullException("Brand is required");

            return new VehicleSpecificationDocument
            {
                Id = auctionId,
                AuctionId = auctionId,
                Model = model,
                Brand = brand,
                DynamicMetadata = dynamicMetadata ?? new Dictionary<string, object>()
            };
        }

        public void UpdateMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Metadata key cannot be empty");

            DynamicMetadata[key] = value;
        }
    }
}
