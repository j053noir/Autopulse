using AutoPulse.Application.Application.Common.Interfaces;
using MediatR;

namespace AutoPulse.Application.Application.Common.Behaviors
{
    public class ReadOnlyDatabaseBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IReadOnlyQuery<TResponse>
    {
        private readonly IAutoPulseDbContext _dbContext;

        public ReadOnlyDatabaseBehavior(IAutoPulseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TResponse> Handle
        (
            TRequest request, 
            RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken
        )
        {
            _dbContext.UseReadOnlyDatabase(true);

            return await next();
        }
    }
}
