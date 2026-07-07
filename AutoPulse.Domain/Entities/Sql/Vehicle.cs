using AutoPulse.Domain.Common;

namespace AutoPulse.Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        public Auction? Auction { get; private set; }
        public string? VIN { get; private set; }
        public string? Marquee { get; private set; }
        public string? Model { get; private set; }
        public int? Year { get; private set; }
        public int? Mileage { get; private set; }

        public Vehicle() { }

        private Vehicle(Guid id, string vin, string marquee, string model, int year, int mileage)
        {
            Id = id;
            VIN = vin;
            Marquee = marquee;
            Model = model;
            Year = year;
            Mileage = mileage;
        }

        public static Vehicle Create(Guid id, string vin, string marquee, string model, int year, int mileage)
        {
            if (id == Guid.Empty) throw new ArgumentException("id is required");

            if (string.IsNullOrWhiteSpace(vin) || vin.Length != 17)
                throw new ArgumentException("VIN must be exactly 17 characters.", nameof(vin));

            if (year > DateTime.UtcNow.Year + 1)
                throw new ArgumentException("Invalid Vehicle year", nameof(year));

            if (mileage < 0)
                throw new ArgumentException("Mileage cannot be negative.", nameof(mileage));

            return new Vehicle(id, vin, marquee, model, year, mileage);
        }
    }
}
