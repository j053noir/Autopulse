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
        private readonly IUserProfileService _userProfileService;
        private readonly IAuctionEventDispatcher _dispatcher;

        public CreateAuctionBidCommandHandler
        (
            IRepository<Auction> auctionRepository,
            IAutoPulseDbContext context,
            ICacheService cacheService,
            IRepository<User> userRepository,
            IUserProfileService userProfileService,
            IAuctionEventDispatcher dispatcher
        )
        {
            _auctionRepository = auctionRepository;
            _context = context;
            _cacheService = cacheService;
            _userRepository = userRepository;
            _userProfileService = userProfileService;
            _dispatcher = dispatcher;
        }

        public async Task<Guid> Handle(CreateAuctionBidCommand request, CancellationToken cancellationToken)
        {
            // 1. Retrieve auction, or throw if not found
            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId)
                ?? throw new KeyNotFoundException($"Auction with id: {request.AuctionId} was not found");

            // 2. Create the inmutable value object for the amount
            var bidAmount = Money.Create(request.Amount, request.Currency);

            // 3. Create bid through parent auction
            var bid = auction.PlaceBid(request.BidderId, bidAmount);

            try
            {
                // 5. Save changes to the database
                await _context.SaveChangesAsync(cancellationToken);

                var cacheKey = CacheKeys.AuctionDetail(request.AuctionId);
                // 6.1. Invalidate the cache for the user's bids
                var userBidsCacheKey = CacheKeys.UserBids(request.BidderId);
                await _cacheService.RemoveAsync(userBidsCacheKey, cancellationToken);

                // 6.2. Invalidate the active auctions list cache
                await _cacheService.RemoveAsync(CacheKeys.ActiveAuctionsList, cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // 7. Handle the conflict gracefully
                throw new Exception("The auction was updated by another user while you were placing your bid. Please try again.");
            }

            // 8. Retrieve bidder data using UserProfileService
            var bidder = await _userProfileService.GetProfileAsync(request.BidderId, string.Empty, cancellationToken);

            // 9. Dispatch real time event
            await _dispatcher.PublishBidPlaceAsync(request.AuctionId.ToString(), request.Amount, bidder?.UserName ?? request.BidderId.ToString());

            // 10. Return the new resource Id
            return bid.Id;
        }
    }
}
