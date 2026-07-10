namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface IPermissionCacheService
    {
        /// <summary>
        /// Caches the user's permissions for the specified token family and updates the user's active session index.
        /// The index tracks all active family IDs for the user so they can be invalidated collectively.
        /// </summary>
        Task ServicePermissionsAsync(Guid userId, string familyId, HashSet<string> permissions, TimeSpan ttl, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the cached set of permissions for a specific user and token family session.
        /// </summary>
        Task<HashSet<string>?> GetPermissionsAsync(Guid userId, string familyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes the cached permissions for a single specific token family session.
        /// </summary>
        Task RevokeUserAsync(Guid userId, string familyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes all active session permissions for a user by querying the session index 
        /// and deleting each cached session individually, then cleaning up the index itself.
        /// </summary>
        Task InvalidateAllUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
