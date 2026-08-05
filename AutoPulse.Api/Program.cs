using AutoPulse.Api.Hubs;
using AutoPulse.Api.Middleware;
using AutoPulse.Api.Services;
using AutoPulse.Domain.Interfaces;
using AutoPulse.Application.Application.Auctions.Commands.CreateAuction;
using AutoPulse.Application.Application.Common.Behaviors;
using AutoPulse.Infrastructure;
using AutoPulse.Infrastructure.Resilience;
using AutoPulse.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Enrichers.Span;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
    .Enrich.WithProperty("ApplicationName", "AutoPulse.Api")
    .WriteTo.Console(new JsonFormatter()));

// Configure OpenTelemetry Tracing
Sdk.SetDefaultTextMapPropagator(new OpenTelemetry.Context.Propagation.TraceContextPropagator());

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("AutoPulse.Backend"))
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("AutoPulse.*")
            .AddSource("MassTransit")
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddOtlpExporter(options =>
            {
                var endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317";
                options.Endpoint = new Uri(endpoint);
            });
    });

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Exception handling middleware
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddResiliencePipelines();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Intentar extraer el token de la cookie segura HTTP-Only
            if (context.Request.Cookies.TryGetValue("autopulse-session", out var token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddDistributedRateLimiter(builder.Configuration);

builder.Services.AddAuthorization();

// Real-Time events dispatcher registration
builder.Services.AddSignalR();
builder.Services.AddSingleton<IAuctionEventDispatcher, SignalRAuctionEventDispatcher>();

builder.Services.AddMediatR(cfg =>
{
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));

    // Database behavior
    cfg.AddOpenBehavior(typeof(ReadOnlyDatabaseBehavior<,>));

    // Auction
    cfg.RegisterServicesFromAssembly(typeof(CreateAuctionCommand).Assembly);

});

builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration["App:Url"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseRateLimiter();
app.UseAuthentication();

// Middleware to enrich Serilog LogContext with UserId from JWT
app.Use(async (context, next) =>
{
    var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                 ?? context.User.FindFirst("sub")?.Value;
    if (!string.IsNullOrEmpty(userId))
    {
        using (Serilog.Context.LogContext.PushProperty("UserId", userId))
        {
            await next();
        }
    }
    else
    {
        await next();
    }
});

app.UseAuthorization();

app.MapControllers();
app.MapHubs();

app.MapGet("/", () => Results.Content("<h1>Welcome to the AutoPulse API!</h1><p><a href=\"/swagger\">Go to Swagger UI</a></p>", "text/html"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
        options.EnablePersistAuthorization();
    });
}

app.Run();

public sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var bearerScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes.Add("Bearer", bearerScheme);

            var securityRequirement = new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            };

            if (document.Paths is not null)
            {
                foreach (var operation in document.Paths.Values.SelectMany(p => p.Operations ?? []))
                {
                    operation.Value.Security ??= new List<OpenApiSecurityRequirement>();
                    operation.Value.Security.Add(securityRequirement);
                }
            }
        }
    }
}

public partial class Program { }

