using AutoPulse.Domain.Entities;

namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface IJwtProvider
    {
        string GenerateToken(User user);
    }
}
