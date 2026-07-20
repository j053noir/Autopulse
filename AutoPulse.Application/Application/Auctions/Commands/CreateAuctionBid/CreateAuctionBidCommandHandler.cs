using AutoPulse.Application.Application.Common.Constants;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using AutoPulse.Domain.Interfaces;
using AutoPulse.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AutoPulse.Application.Application.Auctions.Commands.CreateAuctionBid
{
    public class CreateAuctionBidCommandHandler : IRequestHandler<CreateAuctionBidCommand, Guid>
    {
        private readonly IRepository<Auction> _auctionRepository;
        private readonly IAutoPulseDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly IRepository<User> _userRepository;
        private readonly IAuctionEventDispatcher _dispatcher;

        public CreateAuctionBidCommandHandler
        (
            IRepository<Auction> auctionRepository,
            IAutoPulseDbContext context,
            ICacheService cacheService,
            IRepository<User> userRepository
        )
        {
            _auctionRepository = auctionRepository;
            _context = context;
            _cacheService = cacheService;
            _userRepository = userRepository;
        }

        public async Task<Guid> Handle(CreateAuctionBidCommand request, CancellationToken cancellationToken)
        {
            // 1. Retrieve auction, or throw if not found
            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId)
                ?? throw new KeyNotFoundException($"Auction with id: {request.AuctionId} was not found");

            // 2. Create the inmutable value object for the amount
            var bidAmount = Money.Create(request.Amount, request.Currency);

            // 3. Retrieve bidder data
            var bidder = await _cacheService.GetAsync<User>(CacheKeys.UserProfile(request.BidderId), cancellationToken);

            // 3. Create bid through parent auction
            var bid = auction.PlaceBid(request.BidderId, bidAmount);

            try
            {
                // 5. Save changes to the database
                await _context.SaveChangesAsync(cancellationToken);

                var cacheKey = CacheKeys.AuctionDetail(request.AuctionId);
                // 6. Invalidate the cache for the auction details
                await _cacheService.RemoveAsync(cacheKey, cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // 7. Handle the conflict gracefully
                throw new Exception("The auction was updated by another user while you were placing your bid. Please try again.");
            }

            // 8. Dispatch real time event
            await _dispatcher.PublishBidPlaceAsync(request.AuctionId.ToString(), request.Amount, request.BidderId.ToString());

            // 9. Return the new resource Id
            return bid.Id;
        }
    }
}
