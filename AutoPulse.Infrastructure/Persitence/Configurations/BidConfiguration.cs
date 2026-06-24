using AutoPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPulse.Infrastructure.Persitence.Configurations
{
    internal class BidConfiguration : IEntityTypeConfiguration<Bid>
    {
        public void Configure(EntityTypeBuilder<Bid> builder)
        {
            builder.ToTable("Bids");

            builder.HasKey(b => b.Id);

            builder.OwnsOne(b => b.Amount, amount =>
            {
                amount.Property(p => p.Amount).HasColumnName("BidAmount").HasPrecision(18, 2);
                amount.Property(p => p.CurrencyCode).HasColumnName("BidCurrencyCode").HasMaxLength(3);
            });
        }
    }
}
