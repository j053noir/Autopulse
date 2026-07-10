using AutoPulse.Application.Application.Authentication.Common.Specification;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Application.Application.Authentication.Commands.LogoutUser
{
    public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, bool>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPermissionCacheService _permissionCacheService;
        private readonly IAutoPulseDbContext _dbContext;
        private readonly ILogger<LogoutUserCommandHandler> _logger;

        public LogoutUserCommandHandler
        (
            IRepository<User> userRepository,
            IJwtProvider jwtProvider,
            IPermissionCacheService permissionCacheService,
            IAutoPulseDbContext dbContext,
            ILogger<LogoutUserCommandHandler> logger
        )
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _permissionCacheService = permissionCacheService;
            _dbContext = dbContext;
            _logger = logger;
        }


        public async Task<bool> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
        {
            var principal = _jwtProvider.GetPrincipalFromAccessToken(request.AccessToken);
            if (principal is null) return false;

            var userId = Guid.Parse(principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var tokenHash = _jwtProvider.HashToken(request.RefreshToken);

            var spec = new ActiveUserSpecificationByIdWithRefreshTokens(userId);
            var user = await _userRepository.GetBySpecAsync(spec, cancellationToken);
            if (user is null) return false;

            var targetToken = user.RefreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash);
            if (targetToken is not null)
            {
                user.RevokeTokenFamily(targetToken.FamilyId);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _permissionCacheService.RevokeUserAsync(userId, targetToken!.FamilyId, cancellationToken);

            return true;
        }
    }
}
