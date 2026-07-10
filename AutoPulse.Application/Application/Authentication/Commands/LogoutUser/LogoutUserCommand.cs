using MediatR;

namespace AutoPulse.Application.Application.Authentication.Commands.LogoutUser
{
    public record LogoutUserCommand(
        string AccessToken,
        string RefreshToken
    ) : IRequest<bool>;
}
