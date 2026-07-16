using AutoPulse.Application.Application.Authentication.Common.Dto;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoPulse.Application.Application.Authentication.Queries.GetCurrentUserProfile
{
    public class GetCurrentUserProfileQueryHandler : IRequestHandler<GetCurrentUserProfileQuery, UserProfileDto?>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IPermissionCacheService _permissionCacheService;

        public GetCurrentUserProfileQueryHandler(
            IRepository<User> userRepository,
            IPermissionCacheService permissionCacheService
        )
        {
            _userRepository = userRepository;
            _permissionCacheService = permissionCacheService;
        }

        public async Task<UserProfileDto?> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null) return null;

            // Fetch permissions from cache or fallback to database values if cache is empty
            var cachedPermissions = await _permissionCacheService.GetPermissionsAsync(request.UserId, request.FamilyId, cancellationToken);
            var permissionSet = cachedPermissions ?? user.Permissions.ToHashSet();

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
