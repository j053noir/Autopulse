using AutoPulse.Domain.Common.Specification;
using AutoPulse.Domain.Entities;

namespace AutoPulse.Application.Application.Authentication.Common.Specification
{
    public class ActiveUserSpecificationByEmailAndPassword : BaseSpecification<User>
    {
        public ActiveUserSpecificationByEmailAndPassword(string username, string passwordHash) :
            base(
                a => a.IsActive.Value &&
                    (!string.IsNullOrWhiteSpace(username) && a.UserName.ToLower() == username.ToLower()) &&
                    (!string.IsNullOrWhiteSpace(passwordHash) && a.PasswordHash == passwordHash)
            )
        {
        }
    }
}
