using System.Linq.Expressions;

namespace AutoPulse.Domain.Common.Specification
{
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        protected BaseSpecification(Expression<Func<T, bool>>? criteria = null)
        {
            Criteria = criteria;
        }

        public Expression<Func<T, bool>>? Criteria { get; protected set; }
        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public Expression<Func<T, object>>? OrderBy { get; protected set; }
        public Expression<Func<T, object>>? OrderByDescending { get; protected set; }

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression) => OrderBy = orderByExpression;
        protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression) => OrderByDescending = orderByDescExpression;
    }
}
