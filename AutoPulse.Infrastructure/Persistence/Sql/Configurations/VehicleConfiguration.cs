using AutoPulse.Domain.Entities;
using AutoPulse.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPulse.Infrastructure.Persistence.Sql.Configurations
{
    internal class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("Vehicles");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.VIN)
                .HasConversion(
                    v => v.Value,
                    v => Vin.Create(v))
                .HasMaxLength(17)
                .IsRequired();

            builder.HasIndex(v => v.VIN).IsUnique();

            builder.Property(v => v.Title)
                .HasMaxLength(255)
                .IsRequired();

            builder.OwnsOne(v => v.BasePrice, price =>
            {
                price.Property(p => p.Amount).HasColumnName("BasePriceAmount").HasPrecision(18, 2);
                price.Property(p => p.CurrencyCode).HasColumnName("BasePriceCurrencyCode").HasMaxLength(3);
            });

            builder.OwnsOne(v => v.MinimumBidIncrement, price =>
            {
                price.Property(p => p.Amount).HasColumnName("MinimumBidIncrementAmount").HasPrecision(18, 2);
                price.Property(p => p.CurrencyCode).HasColumnName("MinimumBidIncrementCurrencyCode").HasMaxLength(3);
            });

            builder.Property(v => v.Category)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(v => v.DocumentStorageKey)
                .HasMaxLength(255)
                .IsRequired();

            // 1. Configure 1:N relationship with Auctions
            builder.HasOne(v => v.Auction)
                    .WithOne(a => a.Vehicle)
                    .HasForeignKey<Auction>(a => a.VehicleId)
                    .IsRequired();
        }
    }
}

