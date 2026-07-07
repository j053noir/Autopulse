using AutoPulse.Domain.Entities;
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

            builder.HasIndex(v => v.VIN).IsUnique();

            // 1. Configure 1:N relationship with Auctions
            builder.HasOne(v => v.Auction)
                    .WithOne(a => a.Vehicle)
                    .HasForeignKey<Auction>(a => a.VehicleId)
                    .IsRequired();
        }
    }
}
