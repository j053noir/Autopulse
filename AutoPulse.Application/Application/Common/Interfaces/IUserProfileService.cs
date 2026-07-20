using AutoPulse.Application.Application.Authentication.Common.Dto;

namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileDto?> GetProfileAsync(Guid userId, string familyId, CancellationToken cancellationToken = default);
    }
}
