namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface IPermissionCacheService
    {
        Task ServicePermissionsAsync(Guid userId, HashSet<string> permissions, TimeSpan ttl, CancellationToken cancellationToken = default);
        Task<HashSet<string>?> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task RevokeUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
