using AutoPulse.Application.Application.Common.Interfaces;

namespace AutoPulse.Infrastructure.Security
{
    internal class BCryptPasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;
        public string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password), "Password cannot be empty");

            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);
        }

        public bool Verify(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
        }
    }
}
