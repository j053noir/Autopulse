using AutoPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPulse.Infrastructure.Persitence.Configurations
{
    internal class AuctionConfiguration : IEntityTypeConfiguration<Auction>
    {
        public void Configure(EntityTypeBuilder<Auction> builder)
        {
            builder.ToTable("Auctions");

            builder.HasKey(a => a.Id);

            // 1. Map "Starting Price" Value Object as Owned Type
            builder.OwnsOne(a => a.StartingPrice, price =>
            {
                price.Property(p => p.Amount).HasColumnName("StartingPriceAmount").HasPrecision(18, 2);
                price.Property(p => p.CurrencyCode).HasColumnName("StartingPriceCurrencyCode").HasMaxLength(3);
            });

            // 2. Map "Current Price" Value Object as Owned Type
            builder.OwnsOne(a => a.CurrentPrice, price =>
            {
                price.Property(p => p.Amount).HasColumnName("CurrentPriceAmount").HasPrecision(18, 2);
                price.Property(p => p.CurrencyCode).HasColumnName("CurrentPriceCurrencyCode").HasMaxLength(3);
            });

            // 3. Configure 1:1 relationship with Vehicle
            builder.HasOne(a => a.Vehicle)
                    .WithMany()
                    .HasForeignKey(a => a.VehicleId)
                    .IsRequired();

            // 4. Configure 1:N relationship with Bids
            builder.HasMany(a => a.Bids)
                    .WithOne(b => b.Auction)
                    .HasForeignKey(b => b.AuctionId)
                    .OnDelete(DeleteBehavior.Cascade);

            // 5. Configure concurrency token for optimistic locking
            builder.Property(a => a.RowVersion)
                    .IsRowVersion();

        }

    }
}
