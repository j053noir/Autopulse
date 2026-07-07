using AutoPulse.Domain.Common.Specification;
using Microsoft.EntityFrameworkCore;

namespace AutoPulse.Infrastructure.Persistence.Sql.Repositories
{
    public static class SpecificationEvaluator<T> where T : class
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
        {
            var query = inputQuery;

            if (specification.Criteria != null) query = query.Where(specification.Criteria);

            // Apply includes
            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

            if (specification.OrderBy != null) query = query.OrderBy(specification.OrderBy);
            else if (specification.OrderByDescending != null) query = query.OrderByDescending(specification.OrderByDescending);
            
            return query;
        }
    }
}
