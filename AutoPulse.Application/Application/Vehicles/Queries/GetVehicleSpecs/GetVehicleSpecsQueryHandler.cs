using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Application.Application.Vehicles.Queries.GetVehicleSpecs.Dto;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities.NoSql;
using MediatR;
using System.Collections.Concurrent;

namespace AutoPulse.Application.Application.Vehicles.Queries.GetVehicleSpecs
{
    public class GetVehicleSpecsQueryHandler : IRequestHandler<GetVehicleSpecsQuery, VehicleSpecsDto?>
    {
        private readonly INoSqlRepository<VehicleSpecificationDocument> _vehicleRepository;
        private readonly ICacheService _cacheService;

        // Static lock to prevent cache stampede
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

        public GetVehicleSpecsQueryHandler(
            INoSqlRepository<VehicleSpecificationDocument> vehicleRepository, 
            ICacheService cacheService)
        {
            _vehicleRepository = vehicleRepository;
            _cacheService = cacheService;
        }

        public async Task<VehicleSpecsDto?> Handle(GetVehicleSpecsQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = GetCacheKey(request);

            // 1. Try to get from cache first
            var cachedSpecs = await _cacheService.GetAsync<VehicleSpecsDto>(cacheKey, cancellationToken);
            if (cachedSpecs is not null)
            {
                return cachedSpecs;
            }

            // 2. Lock to prevent multiple concurrent DB queries for the same AuctionId
            var semaphore = Locks.GetOrAdd(request.AuctionId, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync(cancellationToken);

            try
            {
                // 3. Double-check cache inside the lock
                cachedSpecs = await _cacheService.GetAsync<VehicleSpecsDto>(cacheKey, cancellationToken);
                if (cachedSpecs is not null)
                {
                    return cachedSpecs;
                }

                // 4. Fetch from NoSQL DB
                var doc = await _vehicleRepository.GetByIdAsync(request.AuctionId, cancellationToken);
                if (doc is null) throw new KeyNotFoundException($"Vehicle for AuctionId {request.AuctionId} not found");

                var vehicleSpecsDto = VehicleSpecMapper.Map(doc);

                // 5. Store in cache for 5 minutes (or whatever duration matches your SLA)
                await _cacheService.SetAsync(cacheKey, vehicleSpecsDto, TimeSpan.FromMinutes(5), cancellationToken);

                return vehicleSpecsDto;
            }
            finally
            {
                semaphore.Release();

                // Clean up the dictionary lock if there are no other waiting threads
                if (semaphore.CurrentCount > 0)
                {
                    Locks.TryRemove(request.AuctionId, out _);
                }
            }
            
        }

        private string GetCacheKey(GetVehicleSpecsQuery request)
        {
            return $"vehicles:specs:{request.AuctionId}";
        }
    }
}
