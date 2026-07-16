using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
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

        public CreateAuctionBidCommandHandler
        (
            IRepository<Auction> auctionRepository,
            IAutoPulseDbContext context,
            ICacheService cacheService
        )
        {
            _auctionRepository = auctionRepository;
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<Guid> Handle(CreateAuctionBidCommand request, CancellationToken cancellationToken)
        {
            // 1. Retrieve auction, or throw if not found
            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId)
                ?? throw new KeyNotFoundException($"Auction with id: {request.AuctionId} was not found");

            // 2. Create the inmutable value object for the amount
            var bidAmount = Money.Create(request.Amount, request.Currency);

            // 3. Create bid through parent auction
            var bid = auction.PlaceBid(request.AuctioneerId, bidAmount);

            try
            {
                // 5. Save changes to the database
                await _context.SaveChangesAsync(cancellationToken);

                var cacheKey = GetCacheKey(request.AuctionId);
                // 6. Invalidate the cache for the auction details
                await _cacheService.RemoveAsync(cacheKey, cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // 7. Handle the conflict gracefully
                throw new Exception("The auction was updated by another user while you were placing your bid. Please try again.");
            }

            // 8. Return the new resource Id
            return bid.Id;
        }

        private string GetCacheKey(Guid auctionId) => $"auctions:detail:{auctionId}";
    }
}
