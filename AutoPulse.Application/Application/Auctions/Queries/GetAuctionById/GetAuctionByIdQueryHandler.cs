using AutoPulse.Application.Application.Auctions.Queries.Common.Dto;
using AutoPulse.Application.Application.Auctions.Queries.Common.Specification;
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
            var spec = new AuctionWithDetailsSpecification(request.Id);
            var entity = await _auctionRepository.GetByIdAsync(spec, cancellationToken);

            // 2. Return null if entity not found
            if (entity == null) return null;

            // 3. Map the entity to the DTO
            var auction = AuctionMapper.Map(entity);

            // 4. Return retrieved data
            return auction;
        }
    }
}
