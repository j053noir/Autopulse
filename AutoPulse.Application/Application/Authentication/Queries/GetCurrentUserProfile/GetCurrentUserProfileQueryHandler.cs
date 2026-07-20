using AutoPulse.Application.Application.Authentication.Common.Dto;
using AutoPulse.Application.Application.Common.Interfaces;
using MediatR;

namespace AutoPulse.Application.Application.Authentication.Queries.GetCurrentUserProfile
{
    public class GetCurrentUserProfileQueryHandler : IRequestHandler<GetCurrentUserProfileQuery, UserProfileDto?>
    {
        private readonly IUserProfileService _userProfileService;

        public GetCurrentUserProfileQueryHandler(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        public async Task<UserProfileDto?> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
        {
            return await _userProfileService.GetProfileAsync(request.UserId, request.FamilyId, cancellationToken);
        }
    }
}
