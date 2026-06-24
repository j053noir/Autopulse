namespace AutoPulse.Domain.Common.Interfaces
{
    public interface IRepository<T> where T: class, IAggregateRoot
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        IQueryable<T> AsQueryable();
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
