using AutoPulse.Notifications.Ingestion.Endpoints;
using AutoPulse.Notifications.Ingestion.Infrastructure;
using AutoPulse.Notifications.Ingestion.Interfaces;
using AutoPulse.Notifications.Shared.Serialization;
using StackExchange.Redis;

var builder = WebApplication.CreateSlimBuilder(args);

// 1. Configure shared AOT serialization
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});

// 2. Register infrastructure clients
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetConnectionString("CacheConnection") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(config);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Inject Kafka Confiuration
builder.Services.AddSingleton<IPreferenceService, ValkeyPreferenceService>();
builder.Services.AddSingleton<INotificationPublisher, KafkaNotificationPublisher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map Notification routes
app.MapNotificationRoutes();

app.Run();
