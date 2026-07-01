using AutoPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPulse.Infrastructure.Persitence.Configurations
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
            builder.HasMany(a => a.Bids)
                    .WithOne(b => b.Bidder)
                    .HasForeignKey(b => b.BidderId)
                    .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
