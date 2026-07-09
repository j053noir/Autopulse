using AutoPulse.Application.Application.Authentication.Common.Specification;
using AutoPulse.Application.Application.Authentication.Queries.LoginUser.Dto;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace AutoPulse.Application.Application.Authentication.Queries.LoginUser
{
    public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, AuthDto?>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;
        private readonly IConfiguration _configuration;
        private readonly IPermissionCacheService _permissionCacheService;
        private readonly ILogger<LoginUserQueryHandler> _logger;

        public LoginUserQueryHandler
            (
                IRepository<User> userRepository,
                IPasswordHasher passwordHasher,
                IJwtProvider jwtProvider,
                IConfiguration configuration,
                IPermissionCacheService permissionCacheService,
                ILogger<LoginUserQueryHandler> logger
            )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
            _configuration = configuration;
            _permissionCacheService = permissionCacheService;
            _logger = logger;
        }

        public async Task<AuthDto?> Handle(LoginUserQuery request, CancellationToken cancellationToken)
        {
            var spec = new ActiveUserSpecificationByEmail(request.Email);

            User? user = null;
            try
            {
                var entities = await _userRepository.ListAsync(spec, cancellationToken);
                user = entities?.FirstOrDefault();
            }
            catch (DbException dEx)
            {
                _logger.LogCritical(dEx, $"Catastrophic failure: Relational database failed at login");
            }

            if (user is null || string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new KeyNotFoundException("User not found");

            var token = _jwtProvider.GenerateToken(user);
            if (!double.TryParse(_configuration.GetRequiredSection("Jwt:DurationInMinutes")?.Value, out double expiresIn)) throw new InvalidOperationException("Missing configuration: Jwt.DurationInMinutes");

            var ttl = TimeSpan.FromMinutes(expiresIn);
            await _permissionCacheService.ServicePermissionsAsync(user.Id, [.. user.Permissions], ttl, cancellationToken);

            var authDto = new AuthDto(token, expiresIn);

            return authDto;
        }
    }
}
