using AutoPulse.Notifications.Workers.BackgroundServices;
using AutoPulse.Notifications.Workers.Infrastructure.Providers.Email;
using AutoPulse.Notifications.Workers.Infrastructure.Providers.Push;
using AutoPulse.Notifications.Workers.Infrastructure.Providers.Sms;
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

builder.Services.AddHttpClient<SendGridEmailProvider>(client =>
{
    client.BaseAddress = new Uri("https://api.sendgrid.com/");
});
builder.Services.AddTransient<AwsSesEmailProvider>();

builder.Services.AddTransient<IEmailProvider>(sp =>
{
    var primaryEmailProvider = sp.GetRequiredService<SendGridEmailProvider>();
    var fallbackEmailProvider = sp.GetRequiredService<AwsSesEmailProvider>();
    var logger = sp.GetRequiredService<ILogger<ResilientEmailProvider>>();

    return new ResilientEmailProvider(primaryEmailProvider, fallbackEmailProvider, logger);
});

builder.Services.AddTransient<FcmPushProvider>();
builder.Services.AddTransient<ApnsPushProvider>();

builder.Services.AddTransient<IPushProvider>(sp =>
{
    var primaryPushProvider = sp.GetRequiredService<FcmPushProvider>();
    var fallbackPushProvider = sp.GetRequiredService<ApnsPushProvider>();
    var logger = sp.GetRequiredService<ILogger<ResilientPushProvider>>();

    return new ResilientPushProvider(logger, primaryPushProvider, fallbackPushProvider);
});

builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

builder.Services.AddHostedService<NotificationBackgroundWorker>();

var host = builder.Build();
host.Run();
