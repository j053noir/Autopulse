using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Infrastructure.Authentication;
using AutoPulse.Infrastructure.Cache;
using AutoPulse.Infrastructure.Notification;
using AutoPulse.Infrastructure.Payments;
using AutoPulse.Infrastructure.Persitence;
using AutoPulse.Infrastructure.Persitence.Repositories;
using AutoPulse.Infrastructure.Security;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.AspNetCore.Authorization;
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

            services.AddScoped<IJwtProvider, JwtProvider>();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisConnection");
            });

            services.AddScoped<ICacheService, ValkeyCacheService>();
            services.AddScoped<IPermissionCacheService, PermissionCacheService>();

            // TODO: Replace MockEmailService and MockPaymentService with real implementations when available
            services.AddScoped<IEmailService, MockEmailService>();
            services.AddScoped<IPaymentService, MockPaymentService>();

            services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
            services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            return services;
        }
    }
}
