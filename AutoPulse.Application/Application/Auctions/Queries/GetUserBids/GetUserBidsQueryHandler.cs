using AutoPulse.Application.Application.Auctions.Queries.Common.Specification;
using AutoPulse.Application.Application.Common.Constants;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using MediatR;
using System.Collections.Concurrent;

namespace AutoPulse.Application.Application.Auctions.Queries.GetUserBids
{
    public class GetUserBidsQueryHandler : IRequestHandler<GetUserBidsQuery, IReadOnlyList<UserBidDto>>
    {
        private readonly IRepository<Auction> _auctionRepository;
        private readonly ICacheService _cacheService;

        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> Locks = new();

        public GetUserBidsQueryHandler(IRepository<Auction> auctionRepository, ICacheService cacheService)
        {
            _auctionRepository = auctionRepository;
            _cacheService = cacheService;
        }

        public async Task<IReadOnlyList<UserBidDto>> Handle(GetUserBidsQuery request, CancellationToken cancellationToken)
        {
            // 1. Verificar si las ofertas del usuario están en la caché
            var cacheKey = CacheKeys.UserBids(request.UserId);
            var cachedBids = await _cacheService.GetAsync<List<UserBidDto>>(cacheKey, cancellationToken);

            if (cachedBids is not null)
            {
                return cachedBids;
            }

            // 2. Controlar la concurrencia en caso de cache miss usando un semáforo por ID de usuario
            var semaphore = Locks.GetOrAdd(request.UserId, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                // 3. Verificar de nuevo la caché tras adquirir el bloqueo
                cachedBids = await _cacheService.GetAsync<List<UserBidDto>>(cacheKey, cancellationToken);
                if (cachedBids is not null)
                {
                    return cachedBids;
                }

                // 4. Buscar en la base de datos a través del repositorio del Agregado Raíz (Auction)
                var spec = new AuctionsWithUserBidsSpecification(request.UserId);
                var auctions = await _auctionRepository.ListAsync(spec, cancellationToken);

                // 5. Mapear y proyectar los datos de las ofertas del usuario
                var userBids = auctions
                    .SelectMany(a => a.Bids
                        .Where(b => b.BidderId == request.UserId)
                        .Select(b => new UserBidDto(
                            b.Id,
                            a.Id,
                            a.Vehicle != null ? $"{a.Vehicle.Year} {a.Vehicle.Marquee} {a.Vehicle.Model}" : "Vehículo",
                            b.Amount?.Amount ?? 0,
                            b.Amount?.CurrencyCode ?? "USD",
                            b.CreatedAt.UtcDateTime,
                            a.CurrentPrice?.Amount ?? 0,
                            a.IsActive ?? false
                        ))
                    )
                    .OrderByDescending(b => b.BidDate)
                    .ToList();

                // 6. Almacenar los resultados en la caché por 5 minutos
                var cacheTime = TimeSpan.FromMinutes(5);
                await _cacheService.SetAsync(cacheKey, userBids, cacheTime, cancellationToken);

                return userBids;
            }
            finally
            {
                semaphore.Release();
                if (semaphore.CurrentCount > 0)
                {
                    Locks.TryRemove(request.UserId, out _);
                }
            }
        }
    }
}
