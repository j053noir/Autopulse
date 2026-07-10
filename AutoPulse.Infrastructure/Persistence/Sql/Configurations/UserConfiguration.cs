using AutoPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPulse.Infrastructure.Persistence.Sql.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            // 1. Unique email by user
            builder.HasIndex(u => u.UserName).IsUnique();

            // 2. Unique email by user
            builder.HasIndex(u => u.Email).IsUnique();

            // 3. Configure 1:N relationship with Bids
            builder.HasMany(u => u.Bids)
                    .WithOne(b => b.Bidder)
                    .HasForeignKey(b => b.BidderId)
                    .OnDelete(DeleteBehavior.Cascade);

            // 4. Default "credit_card" value for preferred payment method
            builder.Property(u => u.PreferredPaymentMethod)
                    .HasDefaultValue("credit_card");

            // 5. Configure 1:N relationship with Refresh Tokens
            builder.HasMany(u => u.RefreshTokens)
                    .WithOne(rt => rt.User)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
