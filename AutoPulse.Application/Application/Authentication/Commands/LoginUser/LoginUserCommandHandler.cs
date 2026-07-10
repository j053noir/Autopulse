using AutoPulse.Application.Application.Authentication.Common.Dto;
using AutoPulse.Application.Application.Authentication.Common.Specification;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Security;

namespace AutoPulse.Application.Application.Authentication.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthDto?>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IAutoPulseDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;
        private readonly IConfiguration _configuration;
        private readonly IPermissionCacheService _permissionCacheService;
        private readonly ILogger<LoginUserCommandHandler> _logger;

        public LoginUserCommandHandler
        (
                IRepository<User> userRepository,
                IAutoPulseDbContext dbContext,
                IPasswordHasher passwordHasher,
                IJwtProvider jwtProvider,
                IConfiguration configuration,
                IPermissionCacheService permissionCacheService,
                ILogger<LoginUserCommandHandler> logger
        )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
            _configuration = configuration;
            _permissionCacheService = permissionCacheService;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<AuthDto?> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Obtain user by specification
            var spec = new ActiveUserSpecificationByEmail(request.Email);

            User? user = null;
            try
            {
                user = await _userRepository.GetBySpecAsync(spec, cancellationToken);
            }
            catch (DbException dEx)
            {
                _logger.LogCritical(dEx, $"Catastrophic failure: Relational database failed at login");
                throw;
            }

            // 2. Validate user credentials
            if (user is null || string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new KeyNotFoundException("User not found");

            // 3. CLose other active sessions on user request
            if (request.CloseActiveSessions)
            {
                user.TerminateAllActiveSessions();

                await _permissionCacheService.InvalidateAllUserPermissionsAsync(user.Id, cancellationToken);

                _logger.LogInformation("User {UserId} requested to terminate all other active sessions during login.", user.Id);
            }

            try
            {
                // 4. Create new refresh token
                var refreshToken = _jwtProvider.GenerateSecureString();
                var hashedRefreshToken = _jwtProvider.HashToken(refreshToken);

                // 5. Add the token to the Aggregate Root
                var tokenFamilyId = user.AddRefreshToken(hashedRefreshToken, TimeSpan.FromDays(7));

                // 6. Generate new access token
                (var accessToken, var expiresIn) = _jwtProvider.GenerateAccessToken(user, tokenFamilyId);

                // 8. Sincronize the dynamic cache of permissions
                var ttl = TimeSpan.FromMinutes(expiresIn);
                await _permissionCacheService.ServicePermissionsAsync(user.Id, tokenFamilyId, [.. user.Permissions], ttl, cancellationToken);

                // 9. Persist changes
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new AuthDto(
                    AccessToken: accessToken,
                    RefreshToken: refreshToken,
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
