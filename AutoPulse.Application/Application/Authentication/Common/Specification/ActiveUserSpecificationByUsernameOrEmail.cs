using AutoPulse.Domain.Common.Specification;
using AutoPulse.Domain.Entities;

namespace AutoPulse.Application.Application.Authentication.Common.Specification
{
    public class ActiveUserSpecificationByUsernameOrEmail : BaseSpecification<User>
    {
        public ActiveUserSpecificationByUsernameOrEmail(string username, string? email) :
            base(
                a => a.IsActive.Value &&
                    (!string.IsNullOrWhiteSpace(username) && a.UserName.ToLower() == username.ToLower()) ||
                    (!string.IsNullOrWhiteSpace(email) && a.Email.ToLower() == email.ToLower())
            )
        {
        }
    }
}
