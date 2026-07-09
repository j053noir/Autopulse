using AutoPulse.Domain.Common;

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
    }
}
