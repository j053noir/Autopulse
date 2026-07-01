using AutoPulse.Application.Application.Auctions.Queries.Common.Dto;
using AutoPulse.Application.Application.Auctions.Queries.Common.Specification;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Queries.ActiveAuctionsWithVehicle
{
    internal class GetAuctionWithVehicleQueryHandler : IRequestHandler<GetAuctionWithVehicleQuery, IReadOnlyList<AuctionDto?>>
    {
        private readonly IRepository<Auction> _auctionRepository;

        public GetAuctionWithVehicleQueryHandler(IRepository<Auction> auctionRepository)
        {
            _auctionRepository = auctionRepository;
        }

        public async Task<IReadOnlyList<AuctionDto?>> Handle(GetAuctionWithVehicleQuery request, CancellationToken cancellationToken)
        {
            var spec = new ActiveAuctionsWithVehiclesSpecification(request.AuctionId, request.AuctioneerId, request.Marquee, request.MinYear, request.MaxPrice);
            var activeAuctions = await _auctionRepository.ListAsync(spec, cancellationToken);

            return activeAuctions.Select(a => AuctionMapper.Map(a)).ToList();
        }
    }
}
