namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface IIdempotentCommand
    {
        Guid IdempotencyKey { get; }
    }
}
