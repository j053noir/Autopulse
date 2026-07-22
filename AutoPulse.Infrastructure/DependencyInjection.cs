using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Application.Application.Common.Interfaces.Queries;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Infrastructure.Authentication;
using AutoPulse.Infrastructure.Cache;
using AutoPulse.Infrastructure.Messaging;
using AutoPulse.Infrastructure.Messaging.Sagas;
using AutoPulse.Infrastructure.Notification;
using AutoPulse.Infrastructure.Payments;
using AutoPulse.Infrastructure.Persistence.NoSql.Repositories;
using AutoPulse.Infrastructure.Persistence.Sql;
using AutoPulse.Infrastructure.Persistence.Sql.Queries;
using AutoPulse.Infrastructure.Persistence.Sql.Repositories;
using AutoPulse.Infrastructure.Security;
using AutoPulse.Infrastructure.Services.Telemetry;
using AutoPulse.Infrastructure.Services;
using AutoPulse.Domain.Interfaces.Storage;
using EntityFramework.Exceptions.PostgreSQL;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace AutoPulse.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AutoPulseDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("MasterDatabaseConnection"));
                options.UseExceptionProcessor();
            });

            // open typeof(,<>) tells to .NET to resolve any combiation of IRepository<T>
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IAutoPulseDbContext>(provider => provider.GetRequiredService<AutoPulseDbContext>());

            // MongoDB registration
            services.AddSingleton<IMongoClient>(sp => new MongoClient(configuration.GetConnectionString("MongoConnection") ?? "mongodb://localhost:27017"));
            services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase("autopulse"));
            services.AddScoped(typeof(INoSqlRepository<>), typeof(NoSqlRepository<>));

            // MassTransit registration (InMemory for development/testing)
            services.AddMassTransit(x =>
            {
                x.AddSagaStateMachine<AuctionBookingSaga, AuctionBookingSagaState>()
                    .InMemoryRepository();

                x.AddConsumers(typeof(DependencyInjection).Assembly);

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<IEventBus, EventBus>();
            services.AddScoped<IAuctionQueries, AuctionQuery>();

            services.AddScoped<IJwtProvider, JwtProvider>();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("CacheConnection");
            });

            services.AddScoped<ICacheService, ValkeyCacheService>();
            services.AddScoped<IPermissionCacheService, PermissionCacheService>();
            services.AddScoped<IUserProfileService, UserProfileService>();

            // Register Blob Storage Service
            services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

            // Register HTTP Client for Notifications Ingestion API
            services.AddHttpClient<INotificationClient, NotificationHttpClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["Notifications:ServiceUrl"] ?? "http://localhost:5200");
            });

            // TODO: Replace MockEmailService and MockPaymentService with real implementations when available
            services.AddScoped<IEmailService, MockEmailService>();
            services.AddScoped<IPaymentService, MockPaymentService>();

            services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
            services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.AddSingleton<ITelemetryProcessor, TelemetryProcessor>();

            return services;
        }
    }
}
