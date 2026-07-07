using AutoPulse.Domain.Common;
using AutoPulse.Domain.Common.Interfaces;
using MongoDB.Driver;

namespace AutoPulse.Infrastructure.Persistence.NoSql.Repositories
{
    internal class NoSqlRepository<T> : INoSqlRepository<T> where T : class, IAggregateRoot
    {
        private readonly IMongoCollection<T> _collection;

        public NoSqlRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<T>(typeof(T).Name);
        }

        public async Task<string> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity is BaseDocument doc)
            {
                doc.CreatedAt = DateTimeOffset.UtcNow;
            }

            await _collection.InsertOneAsync(entity);

            if (entity is BaseDocument insertedDoc)
            {
                return insertedDoc.Id;
            }

            var idProperty = typeof(T).GetProperty("Id");
            return idProperty?.GetValue(entity)?.ToString() ?? string.Empty;
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var filter = GetIdFilter(id);

            if (typeof(BaseDocument).IsAssignableFrom(typeof(T)))
            {
                var update = Builders<T>.Update.Set(nameof(BaseDocument.DeletedAt), DateTimeOffset.UtcNow);
                await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            }
            else
            {
                await _collection.DeleteOneAsync(filter, cancellationToken);
            }
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var filter = Builders<T>.Filter.Empty;

            if (typeof(BaseDocument).IsAssignableFrom(typeof(T)))
            {
                filter = GetSoftDeleteFilter();
            }

            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }

        public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var filter = GetIdFilter(id);

            if (typeof(BaseDocument).IsAssignableFrom(typeof(T)))
            {
                var softDeleteFilter = GetSoftDeleteFilter();
            }

            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<string> UpdateAsync(string id, T entity, CancellationToken cancellationToken = default)
        {
            if (entity is BaseDocument doc)
            {
                doc.UpdatedAt = DateTimeOffset.UtcNow;
            }

            var filter = GetIdFilter(id);
            await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);

            return id;
        }

        private FilterDefinition<T> GetIdFilter(string id)
        {
            return Builders<T>.Filter.Eq("_id", id);
        }

        private FilterDefinition<T> GetSoftDeleteFilter()
        {
            return Builders<T>.Filter.Eq<T>(nameof(BaseDocument.DeletedAt), null);
        }
    }
}
