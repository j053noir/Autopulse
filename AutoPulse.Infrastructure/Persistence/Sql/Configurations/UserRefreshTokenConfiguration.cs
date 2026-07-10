using AutoPulse.Domain.Entities.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPulse.Infrastructure.Persistence.Sql.Configurations
{
    public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
    {
        public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
        {
            builder.ToTable("UserRefreshToken");

            builder.HasIndex(rt => rt.TokenHash).IsUnique();

            builder.HasIndex(rt => rt.FamilyId);

            builder.Property(rt => rt.TokenHash).HasMaxLength(256).IsRequired();

            builder.Property(rt => rt.FamilyId).HasMaxLength(128).IsRequired();
        }
    }
}
