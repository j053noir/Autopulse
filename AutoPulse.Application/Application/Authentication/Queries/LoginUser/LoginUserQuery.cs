using AutoPulse.Application.Application.Authentication.Queries.LoginUser.Dto;
using MediatR;

namespace AutoPulse.Application.Application.Authentication.Queries.LoginUser
{
    public record LoginUserQuery(
        string Email,
        string Password
        ) : IRequest<AuthDto?>;
}
