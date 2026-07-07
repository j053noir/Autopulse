namespace AutoPulse.Domain.Common.Interfaces
{
    public interface INoSqlRepository<T> where T : class, IAggregateRoot
    {
        Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<string> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<string> UpdateAsync(string id, T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
