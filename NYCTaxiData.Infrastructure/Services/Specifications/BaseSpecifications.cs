using NYCTaxiData.Domain.Interfaces.Specifications;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications
{
    public abstract class BaseSpecification<TEntity> : ISpecification<TEntity> where TEntity : class
    {
        protected BaseSpecification() { }

        protected BaseSpecification(Expression<Func<TEntity, bool>> criteria)
        {
            Criteria = criteria;
        }

        public Expression<Func<TEntity, bool>>? Criteria { get; private set; }
        public List<Expression<Func<TEntity, object>>> Includes { get; } = new();
        public Expression<Func<TEntity, object>>? OrderBy { get; private set; }
        public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }
        public int? Take { get; private set; }
        public int? Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; }

        protected void AddCriteria(Expression<Func<TEntity, bool>> criteria)
            => Criteria = criteria;

        protected void AddInclude(Expression<Func<TEntity, object>> include)
            => Includes.Add(include);

        protected void AddOrderBy(Expression<Func<TEntity, object>> orderBy)
            => OrderBy = orderBy;

        protected void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDesc)
            => OrderByDescending = orderByDesc;

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }
    }
}
