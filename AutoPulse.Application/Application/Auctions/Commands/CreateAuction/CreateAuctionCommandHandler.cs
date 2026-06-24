using AutoPulse.Application.Application.Common.Interfaces;
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

        public CreateAuctionCommandHandler(IRepository<Auction> auctionRepository, IAutoPulseDbContext context)
        {
            _auctionRepository = auctionRepository;
            _context = context;
        }

        public async Task<Guid> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
        {
            // 1. Create vehicle entity using its factory methods
            var vehicleId = Guid.NewGuid();
            var vehicle = Vehicle.Create(vehicleId, request.Vin, request.Marquee, request.Model, request.Year, request.Mileage);

            // 2. Create auction entity using its factory methods
            var auctionId = Guid.NewGuid();
            var auction = Auction.Create(auctionId, vehicle, Money.CreateCAD(request.StartingPrice), request.EndTime);

            // 3. Add the auction entity to the repository
            _auctionRepository.Add(auction);

            // 4. Save changes to the database
            await _context.SaveChangesAsync(cancellationToken);

            // 5. Return the new resource Id
            return auctionId;
        }
    }
}
