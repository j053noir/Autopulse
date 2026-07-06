using AutoPulse.Application.Application.Common.Events;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Commands.CloseAuction
{
    public class CloseAuctionCommandHandler : IRequestHandler<CloseAuctionCommand, bool>
    {
        private readonly IRepository<Auction> _auctionRepository;
        private readonly IEventBus _eventBus;
        private readonly IAutoPulseDbContext _dbContext;

        public CloseAuctionCommandHandler(
            IRepository<Auction> auctionRepository,
            IEventBus eventBus,
            IAutoPulseDbContext dbContext
        )
        {
            _auctionRepository = auctionRepository;
            _eventBus = eventBus;
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(CloseAuctionCommand request, CancellationToken cancellationToken)
        {
            // 1. Search for the auction by its ID
            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken);
            if (auction == null || auction.IsActive != true) return false;

            // 2. Close the auction
            auction.Close();

            // 3. Save changes to the database
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 4. Create an integration event to notify other services about the auction closure
            var transactionId = Guid.NewGuid();

            if (auction.WinnerId is null || auction.CurrentPrice is null) return false;

            var integrationEvent = new AuctionEndedIntegrationEvent(
                EventId: transactionId,
                AuctionId: auction.Id,
                WinnerId: auction.WinnerId.Value,
                Amount: auction.CurrentPrice.Amount,
                Currency: auction.CurrentPrice.CurrencyCode,
                OccuredOn: DateTime.UtcNow
            );

            // 5. Publish the integration event to the event bus
            await _eventBus.PublishAsync(integrationEvent, cancellationToken);

            return true;
        }
    }
}
