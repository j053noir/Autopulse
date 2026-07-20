using AutoPulse.Application.Application.Authentication.Common.Dto;
using AutoPulse.Application.Application.Common.Constants;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;

namespace AutoPulse.Infrastructure.Authentication
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IPermissionCacheService _permissionCacheService;
        private readonly ICacheService _cacheService;

        public UserProfileService(
            IRepository<User> userRepository,
            IPermissionCacheService permissionCacheService,
            ICacheService cacheService)
        {
            _userRepository = userRepository;
            _permissionCacheService = permissionCacheService;
            _cacheService = cacheService;
        }

        public async Task<UserProfileDto?> GetProfileAsync(Guid userId, string familyId, CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.UserProfile(userId);
            var cachedProfile = await _cacheService.GetAsync<UserProfileDto>(cacheKey, cancellationToken);

            if (cachedProfile is not null)
            {
                var cachedPermissionsFromCache = await _permissionCacheService.GetPermissionsAsync(userId, familyId, cancellationToken);
                if (cachedPermissionsFromCache is null)
                {
                    var userEntity = await _userRepository.GetByIdAsync(userId, cancellationToken);
                    cachedPermissionsFromCache = userEntity?.Permissions.ToHashSet() ?? new HashSet<string>();
                }

                return cachedProfile with { Permissions = cachedPermissionsFromCache };
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return null;

            var cachedPermissions = await _permissionCacheService.GetPermissionsAsync(userId, familyId, cancellationToken);
            var permissionSet = cachedPermissions ?? user.Permissions.ToHashSet();

            var profileWithoutPermissions = new UserProfileDto(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                user.IsActive ?? false,
                null
            );

            await _cacheService.SetAsync(cacheKey, profileWithoutPermissions, TimeSpan.FromMinutes(10), cancellationToken);

            return new UserProfileDto(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                user.IsActive ?? false,
                permissionSet
            );
        }
    }
}
