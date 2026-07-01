namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface IPermissionCacheService
    {
        Task ServicePermissionsAsync(Guid userId, HashSet<string> permissions, TimeSpan ttl);
        Task<HashSet<string>>? GetPermissionsAsync(Guid userId);
        Task RevokeUserAsync(Guid userId);
    }
}
