using AutoPulse.Application.Application.Authentication.Common.Dto;
using MediatR;

namespace AutoPulse.Application.Application.Authentication.Commands.LoginUser
{
    public record LoginUserCommand(
        string Email,
        string Password,
        bool CloseActiveSessions = false
        ) : IRequest<AuthDto?>;
}
