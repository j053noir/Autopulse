using AutoPulse.Application.Application.Auctions.Queries.Common.Dto;
using AutoPulse.Application.Application.Auctions.Queries.Common.Specification;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using AutoPulse.Application.Application.Common.Constants;
using MediatR;
using System.Collections.Concurrent;

namespace AutoPulse.Application.Application.Auctions.Queries.ActiveAuctionsWithVehicle
{
    internal class GetAuctionWithVehicleQueryHandler : IRequestHandler<GetAuctionWithVehicleQuery, IReadOnlyList<AuctionDto?>>
    {
        private readonly IRepository<Auction> _auctionRepository;
        private readonly ICacheService _cacheService;

        // Static storage and thread safe for locks to prevent multiple threads from accessing the same auction data simultaneously
        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> Locks = new();

        public GetAuctionWithVehicleQueryHandler(
            IRepository<Auction> auctionRepository,
            ICacheService cacheService
        )
        {
            _auctionRepository = auctionRepository;
            _cacheService = cacheService;
        }

        public async Task<IReadOnlyList<AuctionDto?>> Handle(GetAuctionWithVehicleQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = CacheKeys.ActiveAuctionsFiltered(request.AuctionId, request.AuctioneerId, request.Marquee, request.MinYear, request.MaxPrice);

            // 1. Check if the auction data is already cached
            var cachedAuctions = await _cacheService.GetAsync<IReadOnlyList<AuctionDto>>(cacheKey, cancellationToken);
 
            if (cachedAuctions is not null)
            {
                return cachedAuctions;
            }
 
            // 2. If not cached, acquire a lock for the specific auction ID to prevent multiple threads from fetching the same data simultaneously
            var semaphore = Locks.GetOrAdd(request.AuctionId, _ => new SemaphoreSlim(1, 1));
 
            // 3. Wait to enter the semaphore (lock) for the specific auction ID
            await semaphore.WaitAsync(cancellationToken);
 
            try
            {
                // 4. Check the cache again after acquiring the lock to see if another thread has already fetched and cached the data
                cachedAuctions = await _cacheService.GetAsync<IReadOnlyList<AuctionDto>>(cacheKey, cancellationToken);
 
                if (cachedAuctions is not null)
                {
                    return cachedAuctions;
                }
 
                // 5. If still not cached, fetch the auctions data from the repository
                var spec = new ActiveAuctionsWithVehiclesSpecification(request.AuctionId, request.AuctioneerId, request.Marquee, request.MinYear, request.MaxPrice);
                var activeAuctions = await _auctionRepository.ListAsync(spec, cancellationToken);
 
                var mappedAuctionDtos = activeAuctions.Select(a => AuctionMapper.Map(a)).ToList();
 
                // 6. Cache the fetched data for future requests
                var cacheExpiration = TimeSpan.FromMinutes(5); // Adjust the cache expiration time as needed
                await _cacheService.SetAsync(cacheKey, mappedAuctionDtos, cacheExpiration, cancellationToken);
 
                return mappedAuctionDtos;
            }
            finally
            {
                // 7. Release the semaphore (lock) for the specific auction ID
                semaphore.Release();
            }
        }
    }
}
