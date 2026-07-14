using AutoPulse.Notifications.Ingestion.Interfaces;
using StackExchange.Redis;

namespace AutoPulse.Notifications.Ingestion.Infrastructure
{
    public class ValkeyPreferenceService : IPreferenceService
    {
        private readonly IConnectionMultiplexer _cache;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

        public ValkeyPreferenceService(IConnectionMultiplexer redis)
        {
            _cache = redis;
        }        

        public async Task<bool> IsChannelAllowedAsync(string userId, string channelId, CancellationToken cancellationToken = default)
        {
            var db = _cache.GetDatabase();

            string key = $"user:optout:{userId}";

            bool isOptedOut = await db.SetContainsAsync(key, channelId);

            return !isOptedOut;
        }
    }
}
