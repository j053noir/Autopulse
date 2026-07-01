using AutoPulse.Domain.Common.Specification;
using AutoPulse.Domain.Entities;

namespace AutoPulse.Application.Application.Authentication.Common.Specification
{
    public class ActiveUserSpecificationByEmail : BaseSpecification<User>
    {
        public ActiveUserSpecificationByEmail(string email) :
            base(
                a => a.IsActive.Value &&
                    !string.IsNullOrWhiteSpace(email) && a.Email.ToLower() == email.ToLower()
            )
        {
        }
    }
}
