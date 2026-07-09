
using AutoPulse.Application.Application.Common.Events;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Common.Specification;
using AutoPulse.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Infrastructure.BackgroundJobs
{
    internal class ActiveExpiredAuctionByGivenDatetime : BaseSpecification<Auction>
    {
        public ActiveExpiredAuctionByGivenDatetime(
            DateTime datetime
        ) :
            base(
                a => (a.IsActive.HasValue && a.IsActive.Value) && a.EndTime <= datetime
            )
        {
        }
    }

    public class AuctionTimeoutWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuctionTimeoutWorker> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuctionTimeoutWorker
        (
            IServiceProvider serviceProvider,
            ILogger<AuctionTimeoutWorker> logger,
            IPublishEndpoint publishEndpoint
        )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Auction Timeout Worker initialized");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var auctionRepository = scope.ServiceProvider.GetRequiredService<IRepository<Auction>>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<IAutoPulseDbContext>();

                    // 1. Search for auctions that are active and are expired
                    var now = DateTime.UtcNow;
                    var spec = new ActiveExpiredAuctionByGivenDatetime(now);
                    var expiredAuctions = await auctionRepository.ListAsync(spec, stoppingToken);

                    if (expiredAuctions != null && expiredAuctions.Count > 0)
                    {
                        foreach (var expiredAuction in expiredAuctions)
                        {
                            _logger.LogInformation("Processing close of auction {AuctionId}...", expiredAuction.Id);

                            // 2. Execute business lofic in the domain rich entity
                            expiredAuction.Close();

                            // 3. Save changes
                            await dbContext.SaveChangesAsync(stoppingToken);

                            if (expiredAuction.WinnerId.HasValue && expiredAuction.CurrentPrice is not null)
                            {
                                var integrationEvent = new AuctionEndedIntegrationEvent(
                                    EventId: Guid.NewGuid(),
                                    AuctionId: expiredAuction.Id,
                                    WinnerId: expiredAuction.WinnerId.Value,
                                    Amount: expiredAuction.CurrentPrice.Amount,
                                    Currency: expiredAuction.CurrentPrice.CurrencyCode,
                                    PaymentMethod: "credit_card",
                                    OccuredOn: now
                                    );

                                await _publishEndpoint.Publish(integrationEvent, stoppingToken);

                                _logger.LogInformation("AuctionEndedIntegrationEvent published successfully for event AuctionId: {AuctionId}", expiredAuction.Id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error happened while trying to process the auctions timeouts");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
