using AutoPulse.Application;
using AutoPulse.Application.Application.Auctions.Commands.CreateAuction;
using AutoPulse.Application.Application.Common.Behaviors;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddMediatR(cfg =>
{
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));

    // Auction
    cfg.RegisterServicesFromAssembly(typeof(CreateAuctionCommand).Assembly);
    
});

var app = builder.Build();

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

var Marques = new List<string> { "Toyonda", "Honta", "Furd", "Shevrolet", "Nessan" };
var Models = new List<string> { "Alpha", "Beta", "Gamma", "Delta", "Iota" };

app.MapGet("/vehicles", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new Vehicle
        (
            Marques[Random.Shared.Next(Marques.Count)],
            Models[Random.Shared.Next(Models.Count)],
            Random.Shared.Next(2019, 2025)
        ))
        .ToArray();
    return forecast;
})
.WithName("GetVehicles");

app.Run();
