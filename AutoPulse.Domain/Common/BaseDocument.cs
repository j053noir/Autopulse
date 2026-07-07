namespace AutoPulse.Domain.Common
{
    public abstract class BaseDocument
    {
        public string Id { get; protected set; } = string.Empty;
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
