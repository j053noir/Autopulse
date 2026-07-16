using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Application.Application.Common.Security;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using AutoPulse.Domain.ValueObjects;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Commands.CreateAuction
{
    public class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand, Guid>
    {
        private readonly IRepository<Auction> _auctionRepository;
        private readonly IAutoPulseDbContext _context;
        private readonly ICacheService _cacheService;

        public CreateAuctionCommandHandler(
            IRepository<Auction> auctionRepository,
            IAutoPulseDbContext context,
            ICacheService cacheService
        )
        {
            _auctionRepository = auctionRepository;
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<Guid> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
        {
            // 1. Create vehicle entity using its factory methods
            var vehicleId = Guid.NewGuid();

            // 1.1 Sanitize string inputs
            var sanitizedVin = request.Vin.SanitizeInput();
            var sanitizedMarquee = request.Marquee.SanitizeInput();
            var sanitizeModel = request.Model.SanitizeInput();

            var vehicle = Vehicle.Create(vehicleId, sanitizedVin, sanitizedMarquee, sanitizeModel, request.Year, request.Mileage);

            // 2. Create auction entity using its factory methods
            var auctionId = Guid.NewGuid();
            var auction = Auction.Create(auctionId, request.AuctioneerId, vehicle, Money.Create(request.StartingPrice, request.Currency), request.EndTime);

            // 3. Add the auction entity to the repository
            _auctionRepository.Add(auction);

            // 4. Save changes to the database
            await _context.SaveChangesAsync(cancellationToken);

            var cacheKey = GetCacheKey();
            await _cacheService.RemoveAsync(cacheKey, cancellationToken); // Invalidate the cache for the list of active auctions

            // 5. Return the new resource Id
            return auctionId;
        }

        private string GetCacheKey() => $"auctions:list_active";
    }
}
