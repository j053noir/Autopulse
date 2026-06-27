using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Infrastructure.Persitence;
using AutoPulse.Infrastructure.Persitence.Repositories;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoPulse.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AutoPulseDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
                options.UseExceptionProcessor();
            });

            // open typeof(,<>) tells to .NET to resolve any combiation of IRepository<T>
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IAutoPulseDbContext>(provider => provider.GetRequiredService<AutoPulseDbContext>());

            return services;
        }
    }
}
