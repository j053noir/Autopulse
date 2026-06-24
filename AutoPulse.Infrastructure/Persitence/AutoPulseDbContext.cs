using AutoPulse.Application.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AutoPulse.Infrastructure.Persitence
{
    public class AutoPulseDbContext : DbContext, IAutoPulseDbContext
    {
        public AutoPulseDbContext(DbContextOptions<AutoPulseDbContext> options) : base(options) { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // EF Core discovers which table to create (Auctions, Vehicles, Bids) 
            // scanning the classes that inherit IEntityTypeConfiguration in this project.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
