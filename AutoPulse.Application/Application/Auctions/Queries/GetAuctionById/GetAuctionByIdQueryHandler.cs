using AutoPulse.Application.Application.Auctions.Queries.Common.Dto;
using AutoPulse.Application.Application.Auctions.Queries.Common.Specification;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using AutoPulse.Application.Application.Common.Constants;
using MediatR;
using System.Collections.Concurrent;

namespace AutoPulse.Application.Application.Auctions.Queries.GetAuctionById
{
    public class GetAuctionByIdQueryHandler : IRequestHandler<GetAuctionByIdQuery, AuctionDto?>
    {
        private readonly IRepository<Auction> _auctionRepository;
        private readonly ICacheService _cacheService;

        // Static storage and thread safe for locks to prevent multiple threads from accessing the same auction data simultaneously
        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> Locks = new();

        public GetAuctionByIdQueryHandler(IRepository<Auction> auctionRepository, ICacheService cacheService)
        {
            _auctionRepository = auctionRepository;
            _cacheService = cacheService;
        }

        public async Task<AuctionDto?> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Check if the auction is already cached
            var cacheKey = CacheKeys.AuctionDetail(request.Id);
            var cachedAuction = await _cacheService.GetAsync<AuctionDto>(cacheKey, cancellationToken);

            if (cachedAuction is not null)
            {
                return cachedAuction == AuctionMapper.NullPlaceholder() ? null : cachedAuction;
            }

            // 2. If not cached, acquire a lock for the specific auction ID to prevent multiple threads from fetching the same data simultaneously
            var semaphore = Locks.GetOrAdd(request.Id, _ => new SemaphoreSlim(1, 1));

            // 3. Wait to enter the semaphore (lock) for the specific auction ID
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                // 4. Check the cache again after acquiring the lock to avoid redundant database queries
                cachedAuction = await _cacheService.GetAsync<AuctionDto>(cacheKey, cancellationToken);

                if (cachedAuction is not null)
                {
                    return cachedAuction == AuctionMapper.NullPlaceholder() ? null : cachedAuction;
                }

                // 5. Query in the DB
                var spec = new AuctionWithDetailsSpecification(request.Id);
                var entity = await _auctionRepository.GetBySpecAsync(spec, cancellationToken);

                // 6. Return null if entity not found
                if (entity is null)
                {
                    var shortTimeSpan = TimeSpan.FromMinutes(2); // Adjust the cache duration as needed
                    await _cacheService.SetAsync(cacheKey, AuctionMapper.NullPlaceholder(), shortTimeSpan, cancellationToken);

                    return null;
                }

                // 7. Map the entity to the DTO
                var auctionDto = AuctionMapper.Map(entity);

                // 8. Cache the retrieved data for future requests
                var timeSpan = TimeSpan.FromMinutes(5); // Adjust the cache duration as needed
                await _cacheService.SetAsync(cacheKey, auctionDto, timeSpan, cancellationToken);

                return auctionDto;
            }
            finally
            {
                semaphore.Release();
                if (semaphore.CurrentCount > 0)
                {
                    Locks.TryRemove(request.Id, out _);
                }
            }
        }
    }
}
