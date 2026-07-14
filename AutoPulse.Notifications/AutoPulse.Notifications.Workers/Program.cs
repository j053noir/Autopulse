using AutoPulse.Notifications.Workers.BackgroundServices;
using AutoPulse.Notifications.Workers.Infrastructure.Providers;
using AutoPulse.Notifications.Workers.Interfaces;
using AutoPulse.Notifications.Workers.Providers;
using AutoPulse.Notifications.Workers.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<TwilioSmsProvider>();
builder.Services.AddTransient<InfobipSmsProvider>();

builder.Services.AddTransient<ISmsProvider>(sp =>
{
    var primarySmsProvider = sp.GetRequiredService<TwilioSmsProvider>();
    var fallbackSmsProvider = sp.GetRequiredService<InfobipSmsProvider>();
    var logger = sp.GetRequiredService<ILogger<ResilientSmsProvider>>();

    return new ResilientSmsProvider(primarySmsProvider, fallbackSmsProvider, logger);
});

builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

builder.Services.AddHostedService<NotificationBackgroundWorker>();

var host = builder.Build();
host.Run();
