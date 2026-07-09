using AutoPulse.Application.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AutoPulse.Infrastructure.Authentication
{
    public class PermissionCacheService : IPermissionCacheService
    {
        private readonly IDistributedCache _cache;
        private const string PermissionCacheKeyPrefix = "auth:user-perms:";

        public PermissionCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<HashSet<string>?> GetPermissionsAsync(Guid userId)
        {
            var cacheKey = getCacheKey(userId);
            var json = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(json)) return null;

            return JsonSerializer.Deserialize<HashSet<string>>(json);
        }

        public async Task RevokeUserAsync(Guid userId)
        {
            var cacheKey = getCacheKey(userId);
            await _cache.RemoveAsync(cacheKey);
        }

        public async Task ServicePermissionsAsync(Guid userId, HashSet<string> permissions, TimeSpan ttl)
        {
            var cacheKey = getCacheKey(userId);
            var json = JsonSerializer.Serialize(permissions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };
            await _cache.SetStringAsync(cacheKey, json, options);
        }

        private string getCacheKey(Guid userId)
        {
            return $"{PermissionCacheKeyPrefix}{userId}";
        }
    }
}
