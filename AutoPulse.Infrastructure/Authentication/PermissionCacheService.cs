using AutoPulse.Application.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AutoPulse.Infrastructure.Authentication
{
    public class PermissionCacheService : IPermissionCacheService
    {
        private readonly IDistributedCache _cache;
        private const string PermissionCacheKeyPrefix = "auth:user-perms";
        private const string UserSessionCacheKeyPrefix = "user-session";

        public PermissionCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<HashSet<string>?> GetPermissionsAsync(Guid userId, string familyId, CancellationToken cancellationToken)
        {
            var cacheKey = GetPermissionByUserFamilyCacheKey(userId, familyId);
            var json = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(json)) return null;

            return JsonSerializer.Deserialize<HashSet<string>>(json);
        }

        public async Task RevokeUserAsync(Guid userId, string familyId, CancellationToken cancellationToken = default)
        {
            var cacheKey = GetPermissionByUserFamilyCacheKey(userId, familyId);
            await _cache.RemoveAsync(cacheKey, cancellationToken);
        }

        public async Task ServicePermissionsAsync(Guid userId, string familyId, HashSet<string> permissions, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            var cacheKey = GetPermissionByUserFamilyCacheKey(userId, familyId);
            var userPermissions = JsonSerializer.Serialize(permissions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };
            // Cache permissions for this specific session family
            await _cache.SetStringAsync(cacheKey, userPermissions, options, cancellationToken);

            // Update the index list of active session families for this user
            var indexKey = GetSessionByUserCacheKey(userId);
            var currentSessions = await GetRegisteredFamiliesAsync(indexKey, cancellationToken);

            if (!currentSessions.Contains(familyId))
            {
                currentSessions.Add(familyId);
                var userSession = JsonSerializer.Serialize(currentSessions);

                await _cache.SetStringAsync(indexKey, userSession, cancellationToken);
            }
        }

        /// <summary>
        /// Generates the cache key for storing a user's permissions under a specific token family.
        /// </summary>
        private string GetPermissionByUserFamilyCacheKey(Guid userId, string familyId)
        {
            return $"{PermissionCacheKeyPrefix}:{userId}:{familyId}";
        }

        /// <summary>
        /// Generates the cache key for the index tracking all active token families (sessions) for a user.
        /// </summary>
        private string GetSessionByUserCacheKey(Guid userId)
        {
            return $"{UserSessionCacheKeyPrefix}:{userId}";
        }

        public async Task InvalidateAllUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var indexKey = GetSessionByUserCacheKey(userId);
            var registeredFamilies = await GetRegisteredFamiliesAsync(indexKey, cancellationToken);

            // Revoke each tracked session family
            foreach (var registeredFamily in registeredFamilies)
            {
                var userPermission = GetPermissionByUserFamilyCacheKey(userId, registeredFamily);
                await _cache.RemoveAsync(userPermission, cancellationToken);
            }

            // Remove the session index itself
            await _cache.RemoveAsync(indexKey, cancellationToken);
        }

        /// <summary>
        /// Retrieves the list of active token family IDs tracked in the user's session index from cache.
        /// </summary>
        private async Task<List<string>> GetRegisteredFamiliesAsync(string indexKey, CancellationToken cancellationToken = default)
        {
            var data = await _cache.GetStringAsync(indexKey, cancellationToken);
            return data is null ? [] : JsonSerializer.Deserialize<List<string>>(data);
        }
    }
}
