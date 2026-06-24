namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface IAutoPulseDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
