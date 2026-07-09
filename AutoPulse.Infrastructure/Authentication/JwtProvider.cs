using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        public string GenerateToken(User user)
        {
            if (user == null) throw new ArgumentException("User is required");

            // 1. Define the claims for the JWT token
            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new (JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                new (JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            };

            // 2. Retrieve the secret key from configuration
            var secretKey = _configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT Secret key is missing in configuration.");

            // 3. Create a symmetric security key using the secret key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // 4. Create signing credentials using the security key and HMAC SHA256 algorithm
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 5. Create the security token descriptor with claims, expiration, issuer, audience, and signing credentials
            var token = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:TokenExpirationMinutes"] ?? "60")),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = credentials
            };

            // 6. Create the JWT token using JwtSecurityTokenHandler
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(token);

            return tokenHandler.WriteToken(securityToken);
        }
    }
}
