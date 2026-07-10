using System.Security;

namespace AutoPulse.Domain.Exceptions
{
    public class TokenCompromisedException : SecurityException
    {
        public string FamilyId { get; private set; }

        public TokenCompromisedException(string familyId, string? message) : base(message)
        {
            FamilyId = familyId;
        }
    }
}
