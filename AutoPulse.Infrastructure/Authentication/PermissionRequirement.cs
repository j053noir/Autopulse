using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace AutoPulse.Infrastructure.Authentication
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionCacheService _cacheService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(
            IPermissionCacheService cacheService,
            IServiceProvider serviceProvider,
            ILogger<PermissionAuthorizationHandler> logger
        )
        {
            _cacheService = cacheService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync
            (
                AuthorizationHandlerContext context,
                PermissionRequirement requirement
            )
        {
            // 1. Extract the user ID from the JWT claims (handling default ASP.NET Core claim mapping)
            var userIdClaim = context.User.Claims.FirstOrDefault(c => 
                c.Type == JwtRegisteredClaimNames.Sub || 
                c.Type == "sub" || 
                c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim?.Value, out var userId)) return;

            HashSet<string> cachedPermissions = null;

            try
            {
                // 2. Retrieve the user's permissions from the cache service
                cachedPermissions = await _cacheService.GetPermissionsAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Security Cache Layer is unvailable. Falling back to relational database for User: {userIdClaim}");

                using var scope = _serviceProvider.CreateScope();

                cachedPermissions = await GetPermissionsFromDatabaseBackupAsync(userId, scope);
            }

            // 3. Check if the user has any permissions cached, if the user permissions can be revoked
            if (cachedPermissions is null) return;

            // 4. Check if the required permission is in the cached permissions
            if (cachedPermissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }

        private async Task<HashSet<string>?> GetPermissionsFromDatabaseBackupAsync(Guid userId, IServiceScope scope)
        {
            try
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IRepository<User>>();

                var user = await userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("Fallback database backup failed: User {UserId} not found.", userId);
                    return null;
                }

                return user?.Permissions.ToHashSet();
            }
            catch (Exception dbEx)
            {
                _logger.LogCritical(dbEx, "Catastrophic failure: Relational database backup also failed for User: {UserId}");
                return null;
            }
        }
    }
}
