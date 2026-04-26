namespace NYCTaxiData.Infrastructure.Services.Specifications
{
    public abstract class BaseSpecification<TEntity> : NYCTaxiData.Application.Common.Specifications.BaseSpecification<TEntity> where TEntity : class
    {
        protected BaseSpecification()
        {
        }

        protected BaseSpecification(System.Linq.Expressions.Expression<System.Func<TEntity, bool>> criteria)
            : base(criteria)
        {
        }
    }
}
