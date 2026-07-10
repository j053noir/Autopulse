using AutoPulse.Application.Application.Authentication.Common.Dto;
using AutoPulse.Application.Application.Authentication.Common.Specification;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Common.Security;
using AutoPulse.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security;

namespace AutoPulse.Application.Application.Authentication.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthDto?>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IAutoPulseDbContext _dbContext;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;
        private readonly IPermissionCacheService _permissionCacheService;
        private const int GracePeriodSeconds = 5;

        public RefreshTokenCommandHandler
        (
            IRepository<User> userRepository,
            IJwtProvider jwtProvider,
            IAutoPulseDbContext dbContext,
            ILogger<RefreshTokenCommandHandler> logger,
            IPermissionCacheService permissionCacheService)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _dbContext = dbContext;
            _logger = logger;
            _permissionCacheService = permissionCacheService;
        }

        public async Task<AuthDto?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = _jwtProvider.GetPrincipalFromAccessToken(request.AccessToken);
            if (principal is null) return null;


            var userId = Guid.Parse(principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var inputHash = _jwtProvider.HashToken(request.RefreshToken);

            var spec = new ActiveUserSpecificationByIdWithRefreshTokens(userId);
            var user = await _userRepository.GetBySpecAsync(spec, cancellationToken);
            if (user is null) return null;

            var familyIdClaim = principal.FindFirst(Permissions.Claims.FamilyId);
            if (familyIdClaim is null || string.IsNullOrEmpty(familyIdClaim.Value)) return null;

            try
            {
                (var newAccessToken, var expiresIn) = _jwtProvider.GenerateAccessToken(user, familyIdClaim!.Value);

                var (NewTokenStr, NewTokenHash, FamilyId) = user.RotateRefreshToken(
                    inputHash,
                    newAccessToken,
                    () => _jwtProvider.GenerateSecureString(),
                    tokenStr => _jwtProvider.HashToken(tokenStr),
                    GracePeriodSeconds
                );

                await _dbContext.SaveChangesAsync(cancellationToken);

                var ttl = TimeSpan.FromMinutes(expiresIn);
                await _permissionCacheService.ServicePermissionsAsync(userId, FamilyId, [.. user.Permissions], ttl, cancellationToken);

                return new AuthDto(
                    AccessToken: newAccessToken,
                    RefreshToken: NewTokenStr,
                    ExpiresIn: expiresIn
                );
            }
            catch (SecurityException secEx)
            {
                _logger.LogWarning(secEx, "Security Intervention executed during token rotation");
                throw;
            }
        }
    }
}
