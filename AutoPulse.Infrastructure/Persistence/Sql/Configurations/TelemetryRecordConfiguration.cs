using AutoPulse.Domain.Entities.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPulse.Infrastructure.Persistence.Sql.Configurations;

public class TelemetryRecordConfiguration : IEntityTypeConfiguration<TelemetryRecord>
{
    public void Configure(EntityTypeBuilder<TelemetryRecord> builder)
    {
        builder.ToTable("TelemetryRecords");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.VehicleId)
            .IsRequired();

        builder.Property(t => t.Speed)
            .IsRequired();

        builder.Property(t => t.Odometer)
            .IsRequired();

        builder.Property(t => t.BatteryLevel)
            .IsRequired();

        builder.Property(t => t.Latitude)
            .IsRequired();

        builder.Property(t => t.Longitude)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Timestamp)
            .IsRequired();

        // Índice para acelerar la consulta de archivado por fecha
        builder.HasIndex(t => t.Timestamp);
    }
}
