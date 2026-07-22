using AutoPulse.Domain.Common;
using AutoPulse.Domain.ValueObjects;

namespace AutoPulse.Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        public Auction? Auction { get; private set; }
        public Vin? VIN { get; private set; }
        public string? Marquee { get; private set; }
        public string? Model { get; private set; }
        public int? Year { get; private set; }
        public int? Mileage { get; private set; }
        public string? Title { get; private set; }
        public Money? BasePrice { get; private set; }
        public Money? MinimumBidIncrement { get; private set; }
        public string? Category { get; private set; }
        public string? DocumentStorageKey { get; private set; }

        public Vehicle() { }

        private Vehicle(
            Guid id, 
            Vin vin, 
            string marquee, 
            string model, 
            int year, 
            int mileage,
            string title,
            Money basePrice,
            Money minimumBidIncrement,
            string category,
            string documentStorageKey)
        {
            Id = id;
            VIN = vin;
            Marquee = marquee;
            Model = model;
            Year = year;
            Mileage = mileage;
            Title = title;
            BasePrice = basePrice;
            MinimumBidIncrement = minimumBidIncrement;
            Category = category;
            DocumentStorageKey = documentStorageKey;
        }

        public static Vehicle Create(
            Guid id, 
            string vin, 
            string marquee, 
            string model, 
            int year, 
            int mileage,
            string title,
            Money basePrice,
            Money minimumBidIncrement,
            string category,
            string documentStorageKey)
        {
            if (id == Guid.Empty) throw new ArgumentException("id is required");
            ArgumentNullException.ThrowIfNull(basePrice);
            ArgumentNullException.ThrowIfNull(minimumBidIncrement);
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category is required", nameof(category));
            if (string.IsNullOrWhiteSpace(documentStorageKey)) throw new ArgumentException("Document storage key is required", nameof(documentStorageKey));

            var vinObj = Vin.Create(vin);

            if (year > DateTime.UtcNow.Year + 1)
                throw new ArgumentException("Invalid Vehicle year", nameof(year));

            if (mileage < 0)
                throw new ArgumentException("Mileage cannot be negative.", nameof(mileage));

            return new Vehicle(id, vinObj, marquee, model, year, mileage, title, basePrice, minimumBidIncrement, category, documentStorageKey);
        }
    }
}

