using AutoPulse.Domain.Common;
using AutoPulse.Domain.Common.Interfaces;

namespace AutoPulse.Infrastructure.Persitence.Repositories
{
    public class Repository<T> : IRepository<T> where T : class, IAggregateRoot
    {
        private readonly AutoPulseDbContext _context;

        public Repository(AutoPulseDbContext context)
        {
            _context = context;
        }

        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public IQueryable<T> AsQueryable()
        {
            return _context.Set<T>().AsQueryable();
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FindAsync([id], cancellationToken);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public async Task ReloadAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _context.Entry(entity).ReloadAsync(cancellationToken);
        }
    }
}
