using AutoPulse.Domain.Entities;

namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface IAutoPulseDbContext
    {
        /// <summary>
        /// Changes the database connection to a read-only database. This is useful for scenarios where you want to offload read operations to a separate database instance, improving performance and scalability.
        /// </summary>
        /// <param name="isReadOnly"></param>
        void UseReadOnlyDatabase(bool isReadOnly);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
