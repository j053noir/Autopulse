using MediatR;

namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface IReadOnlyQuery<out TResponse> : IRequest<TResponse> { }
}
