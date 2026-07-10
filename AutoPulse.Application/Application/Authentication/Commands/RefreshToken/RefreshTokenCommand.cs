using AutoPulse.Application.Application.Authentication.Common.Dto;
using MediatR;

namespace AutoPulse.Application.Application.Authentication.Commands.RefreshToken
{
    public record RefreshTokenCommand
    (
        string AccessToken,
        string RefreshToken
    ) : IRequest<AuthDto?>;
}
