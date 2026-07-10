using AutoPulse.Domain.Common.Specification;
using AutoPulse.Domain.Entities;

namespace AutoPulse.Application.Application.Authentication.Common.Specification
{
    public class ActiveUserSpecificationByIdWithRefreshTokens : BaseSpecification<User>
    {
        public ActiveUserSpecificationByIdWithRefreshTokens(Guid userId) :
            base(u => u.Id == userId)
        {
            AddInclude(u => u.RefreshTokens);
        }
    }
}
