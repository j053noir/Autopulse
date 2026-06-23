using AutoPulse.Application.Application.Common.Interfaces;
using MediatR;

namespace AutoPulse.Application.Application.Common.Behaviors
{
    public class IdempotencyBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IIdempotentCommand
    {
        private static readonly Dictionary<Guid, object> _cache = new();

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var key = request.IdempotencyKey;

            // 1. Check if the response is already cached
            if (_cache.TryGetValue(key, out var cachedResponse))
            {
                return (TResponse)cachedResponse;
            }

            // 2. If not cached, proceed with the request and cache the response
            var response = await next();
            if (response != null) _cache[key] = response;
            else throw new ArgumentNullException("response missing");


            return response;
        }
    }
}
