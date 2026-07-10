using AutoPulse.Domain.Common;

namespace AutoPulse.Domain.Entities.Sql
{
    public class UserRefreshToken : BaseEntity
    {
        public Guid UserId { get; private set; }
        public User? User { get; private set; }
        public string? TokenHash { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public bool? IsUsed { get; private set; }
        public DateTime? UsedAt { get; private set; }
        public bool? IsRevoked { get; private set; }
        public string? FamilyId { get; private set; }
        public string? TargetAccessToken { get; private set; }
        public string? TargetRefreshToken { get; private set; }

        private UserRefreshToken() { }

        public static UserRefreshToken Create(Guid userId, string tokenHash, TimeSpan ttl, string familyId)
        {
            return new UserRefreshToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.Add(ttl),
                IsUsed = false,
                IsRevoked = false,
                FamilyId = string.IsNullOrEmpty(familyId) ? Guid.NewGuid().ToString() : familyId,
            };
        }

        public void MarkAsUsed(string targetAccessToken, string targetRefreshToken)
        {
            IsUsed = true;
            UsedAt = DateTime.UtcNow;
            TargetAccessToken = targetAccessToken;
            TargetRefreshToken = targetRefreshToken;
        }

        public void RevokeFamily() => IsRevoked = true;

    }
}
