using AutoPulse.Application.Application.Authentication.Common.Dto;
using AutoPulse.Application.Application.Common.Interfaces;

namespace AutoPulse.Application.Application.Authentication.Queries.GetCurrentUserProfile
{
    public record GetCurrentUserProfileQuery(Guid UserId, string FamilyId) : IReadOnlyQuery<UserProfileDto?>;
}
