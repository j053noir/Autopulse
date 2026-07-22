using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AutoPulse.Infrastructure.Persistence.Sql
{
    public class AutoPulseDbContextFactory : IDesignTimeDbContextFactory<AutoPulseDbContext>
    {
        public AutoPulseDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../AutoPulse.Api");
            if (!Directory.Exists(basePath))
            {
                basePath = Directory.GetCurrentDirectory();
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("MasterDatabaseConnection") 
                ?? "Host=localhost;Database=autopulse;Username=postgres;Password=postgres";

            var optionsBuilder = new DbContextOptionsBuilder<AutoPulseDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AutoPulseDbContext(optionsBuilder.Options, configuration);
        }
    }
}
