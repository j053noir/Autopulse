using AutoPulse.Domain.Common;
using AutoPulse.Domain.Entities.Sql;
using AutoPulse.Domain.Exceptions;
using System.Security;

namespace AutoPulse.Domain.Entities
{
    public class User : BaseEntity, IAggregateRoot
    {
        public string? UserName { get; private set; }
        public string? Email { get; private set; }
        public bool? IsActive { get; private set; }
        public string? PasswordHash { get; private set; }
        public List<string> Permissions { get; private set; } = [];

        // Encapsulated collection: readonly for external entities
        private readonly List<Bid> _bids = new();
        public IReadOnlyCollection<Bid> Bids => _bids.AsReadOnly();

        public string PreferredPaymentMethod { get; private set; } = "credit_card";

        public readonly List<UserRefreshToken> _refreshTokens = new();
        public IReadOnlyCollection<UserRefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        public User() { }

        private User(Guid id, string? userName, string? email, string passwordHash, List<string> permissions)
        {
            Id = id;
            UserName = userName;
            Email = email;
            PasswordHash = passwordHash;
            CreatedAt = DateTimeOffset.UtcNow;
            Permissions = permissions;
            IsActive = true;
        }

        private User(Guid id, string? userName, string? email, bool? isActive, List<string> permissions, DateTimeOffset? createdAt)
        {
            Id = id;
            UserName = userName;
            Email = email;
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
            Permissions = permissions;
            IsActive = isActive;
        }

        public static User Create(Guid id, string? userName, string? email, string passwordHash, List<string> permissions)
        {
            return new User(id, userName, email, passwordHash, permissions);
        }

        public static User Create(Guid id, string? userName, string? email, bool? isActive, List<string> permissions, DateTimeOffset? createdAt)
        {
            return new User(id, userName, email, isActive ?? false, permissions, createdAt);
        }

        public void UpdatePreferredPaymentMethod(string preferredPaymentMethod)
        {
            if (string.IsNullOrWhiteSpace(preferredPaymentMethod))
                throw new ArgumentException("The payment method cannot be empty.");

            PreferredPaymentMethod = preferredPaymentMethod;
        }

        public string AddRefreshToken(string tokenHash, TimeSpan ttl, string? familyId = null)
        {
            string assignedFamilyId = string.IsNullOrEmpty(familyId) ? Guid.NewGuid().ToString() : familyId;

            var refreshToken = UserRefreshToken.Create(Id, tokenHash, ttl, assignedFamilyId);
            _refreshTokens.Add(refreshToken);

            return assignedFamilyId;
        }

        public (string NewTokenStr, string NewTokenHash, string FamilyId) RotateRefreshToken
        (
            string providedTokenHash,
            string currentAccessToken,
            Func<string> secureStringGenerator,
            Func<string, string> tokenHasher,
            int gracePeriodSeconds
        )
        {
            var storedToken = _refreshTokens.FirstOrDefault(t => t.TokenHash == providedTokenHash);

            if (storedToken == null || storedToken.IsRevoked == true || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new SecurityException("Access denied: Not valid refresh token or expired");
            }

            // Concurrency control
            if (storedToken.IsUsed == true)
            {
                var elapsedSinceFirstUse = DateTime.UtcNow - storedToken.UsedAt;

                if (elapsedSinceFirstUse.HasValue && elapsedSinceFirstUse.Value.TotalSeconds <= gracePeriodSeconds)
                {
                    return (storedToken.TargetRefreshToken, storedToken.TokenHash, storedToken.FamilyId);
                }

                string compromisedFamilyId = storedToken.FamilyId;
                RevokeTokenFamily(storedToken.FamilyId, true);

                throw new TokenCompromisedException(compromisedFamilyId, "Security Alert: Malicious reutilization attempt");
            }

            string newRefreshTokenStr = secureStringGenerator();
            string newRefreshTokenHash = tokenHasher(newRefreshTokenStr);

            // Burn the current refresh token, and pass the newly created
            storedToken.MarkAsUsed(currentAccessToken, newRefreshTokenHash);

            // Add the new active token to the existing family
            var nextRefreshToken = UserRefreshToken.Create(Id, newRefreshTokenHash, TimeSpan.FromDays(7), storedToken.FamilyId);
            _refreshTokens.Add(nextRefreshToken);

            return (newRefreshTokenStr, newRefreshTokenHash, storedToken.FamilyId);
        }

        /// <summary>
        /// Massive revoke of a session
        /// </summary>
        /// <param name="familyId"></param>
        public void RevokeTokenFamily(string familyId, bool throws = false)
        {
            var targetTokens = _refreshTokens.Where(t => t.FamilyId == familyId);
            foreach (var targetToken in targetTokens)
            {
                targetToken.RevokeFamily();
            }

            if (throws) throw new SecurityException("Breach Alert: Malicious reuse attempt detected. All the tokens for the same family were revoked.");
        }

        public void TerminateAllActiveSessions()
        {
            var activeTokens = _refreshTokens.Where(t => t.IsRevoked == false && t.ExpiresAt > DateTime.UtcNow);

            foreach (var activeToken in activeTokens)
            {
                activeToken.RevokeFamily();
            }
        }
    }
}
