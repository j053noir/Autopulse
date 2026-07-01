using AutoPulse.Domain.Common.Specification;

namespace AutoPulse.Domain.Common.Interfaces
{
    public interface IRepository<T> where T: class, IAggregateRoot
    {
        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task ReloadAsync(T entity, CancellationToken cancellationToken = default);
    }
}
