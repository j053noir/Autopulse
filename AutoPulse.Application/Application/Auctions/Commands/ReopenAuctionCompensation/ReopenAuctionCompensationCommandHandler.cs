using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Application.Application.Auctions.Commands.ReopenAuctionCompensation
{
    public class ReopenAuctionCompensationCommandHandler : IRequestHandler<ReopenAuctionCompensationCommand, bool>
    {
        private readonly IRepository<Auction> _auctionRepository;
        private readonly IAutoPulseDbContext _dbContext;
        private readonly ILogger<ReopenAuctionCompensationCommandHandler> _logger;

        public ReopenAuctionCompensationCommandHandler(
            IRepository<Auction> auctionRepository,
            IAutoPulseDbContext dbContext,
            ILogger<ReopenAuctionCompensationCommandHandler> logger
        )
        {
            _auctionRepository = auctionRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<bool> Handle(ReopenAuctionCompensationCommand request, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Reopening Auction {AuctionId} due to payment failure: {ErrorMessage}", request.AuctionId, request.ErrorMessage);

            // 1. Search for the auction by its ID
            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken);
            if (auction == null)
            {
                _logger.LogError("Auction {AuctionId} not found for compensation", request.AuctionId);
                return false;
            }

            // 2. Reopen the auction
            auction.Reopen();

            // 3. Save changes to the database
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
