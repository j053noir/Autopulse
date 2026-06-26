using AutoPulse.Application.Application.Auctions.Queries.Common.Dto;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Queries.GetAuctionById
{
    public class GetAuctionByIdQueryHandler : IRequestHandler<GetAuctionByIdQuery, AuctionDto?>
    {
        private readonly IRepository<Auction> _auctionRepository;
        private readonly IAutoPulseDbContext _context;

        public GetAuctionByIdQueryHandler(IRepository<Auction> auctionRepository, IAutoPulseDbContext context)
        {
            _auctionRepository = auctionRepository;
            _context = context;
        }

        public async Task<AuctionDto?> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Query in the DB
            var entity = await _auctionRepository.GetByIdAsync(request.Id, cancellationToken);

            // 2. Return null if entity not found
            if (entity == null) return null;

            // 3. Map the entity to the DTO
            var auction = new AuctionDto(
                Id: entity.Id,
                Vehicle: new VehicleDto(
                    Id: entity.Vehicle?.Id,
                    VIN: entity.Vehicle?.VIN,
                    Marquee: entity.Vehicle?.Marquee,
                    Model: entity.Vehicle?.Model,
                    Year: entity.Vehicle?.Year,
                    Mileage: entity.Vehicle?.Mileage
                ),
                StartingPrice: entity.StartingPrice?.Amount,
                StartingPriceCurrency: entity.StartingPrice?.CurrencyCode,
                CurrentPrice: entity.CurrentPrice?.Amount,
                CurrentPriceCurrency: entity.CurrentPrice?.CurrencyCode,
                EndTime: entity.EndTime,
                IsActive: entity.IsActive
            );

            // 4. Return retrieved data
            return auction;
        }
    }
}
