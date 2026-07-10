using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Security;
using AutoPulse.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AutoPulse.Infrastructure.Authentication
{
    public class JwtProvider : IJwtProvider
    {
        private readonly IConfiguration _configuration;

        public JwtProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <returns>The generated JWT token.</returns>
        /// <exception cref="ArgumentException">Thrown when the user is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the JWT configuration is missing.</exception>
        public (string, double) GenerateAccessToken(User user, string familyId)
        {
            if (user == null) throw new ArgumentException("User is required");

            // 1. Define the claims for the JWT token
            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new (JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                new (JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(Permissions.Claims.FamilyId, familyId)
            };

            // 2. Retrieve the secret key from configuration
            var secretKey = _configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT Secret key is missing in configuration.");

            // 3. Create a symmetric security key using the secret key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // 4. Create signing credentials using the security key and HMAC SHA256 algorithm
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 5. Create the security token descriptor with claims, expiration, issuer, audience, and signing credentials
            if (!double.TryParse(_configuration.GetRequiredSection("Jwt:DurationInMinutes")?.Value, out double expiresIn)) throw new InvalidOperationException("Missing configuration: Jwt.DurationInMinutes");
            
            var token = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiresIn),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = credentials
            };

            // 6. Create the JWT token using JwtSecurityTokenHandler
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(token);

            return (tokenHandler.WriteToken(securityToken), expiresIn);
        }

        public string GenerateSecureString()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? GetPrincipalFromAccessToken(string accessToken)
        {
            var secretKey = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                // We don't validate here to allow the grace period
                ValidateLifetime = false,
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch(Exception)
            {
                return null;
            }
        }

        public string HashToken(string refreshToken)
        {
            var bytes = Encoding.UTF8.GetBytes(refreshToken);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}

