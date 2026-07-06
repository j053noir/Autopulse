using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace AutoPulse.Infrastructure.Persitence
{
    public class AutoPulseDbContext : DbContext, IAutoPulseDbContext
    {
        private readonly IConfiguration _configuration;
        private bool _isReadonlyMode;

        public AutoPulseDbContext(
            DbContextOptions<AutoPulseDbContext> options,
            IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = _isReadonlyMode
                    ? _configuration.GetConnectionString("SlaveDatabaseConnection")
                    : _configuration.GetConnectionString("MasterDatabaseConnection");

                optionsBuilder.UseNpgsql(connectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }

        public void UseReadOnlyDatabase(bool isReadOnly)
        {
            _isReadonlyMode = isReadOnly;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                            .Entries()
                            .Where(e => e.Entity is BaseEntity &&
                                        (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                var now = DateTimeOffset.UtcNow;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = now;
                        break;

                    case EntityState.Modified:
                        entity.UpdatedAt = now;
                        break;

                    case EntityState.Deleted:
                        // Intercept the hard delete and turn it into a soft delete
                        entry.State = EntityState.Modified;
                        entity.DeletedAt = now;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // EF Core discovers which table to create (Auctions, Vehicles, Bids) 
            // scanning the classes that inherit IEntityTypeConfiguration in this project.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        private static System.Linq.Expressions.LambdaExpression ConvertFilterExpression(Type type)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(type, "e");
            var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
            var nullValue = System.Linq.Expressions.Expression.Constant(null, typeof(DateTimeOffset?));
            var comparison = System.Linq.Expressions.Expression.Equal(property, nullValue);

            return System.Linq.Expressions.Expression.Lambda(comparison, parameter);
        }
    }
}
