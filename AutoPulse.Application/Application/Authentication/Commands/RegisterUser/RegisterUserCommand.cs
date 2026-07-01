using AutoPulse.Application.Application.Common.Interfaces;
using MediatR;

namespace AutoPulse.Application.Application.Authentication.Commands.RegisterUser
{
    public record RegisterUserCommand
        (
            string Username,
            string Email,
            string Password,
            Guid IdempotencyKey
        ) : IRequest<Guid>, IIdempotentCommand;
}
