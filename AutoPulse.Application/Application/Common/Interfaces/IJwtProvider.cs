using AutoPulse.Domain.Entities;
using System.Security.Claims;

namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface IJwtProvider
    {
        (string, double) GenerateAccessToken(User user, string familyId);
        /// <summary>
        /// Generate a plain text refresh token
        /// </summary>
        /// <returns></returns>
        string GenerateSecureString();
        /// <summary>
        /// Hashes the refresh token for secure persistence
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        string HashToken(string refreshToken);
        ClaimsPrincipal? GetPrincipalFromAccessToken(string accessToken);

    }
}
