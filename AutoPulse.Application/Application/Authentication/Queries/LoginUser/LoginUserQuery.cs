using AutoPulse.Application.Application.Authentication.Queries.LoginUser.Dto;
using AutoPulse.Application.Application.Common.Interfaces;

namespace AutoPulse.Application.Application.Authentication.Queries.LoginUser
{
    public record LoginUserQuery(
        string Email,
        string Password
        ) : IReadOnlyQuery<AuthDto?>;
}
