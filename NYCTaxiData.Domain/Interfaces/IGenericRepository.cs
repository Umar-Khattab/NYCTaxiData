using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NYCTaxiData.Domain.Common.Interfaces
{
    public interface IGenericRepository<T> where T : class
    { 
        Task<IEnumerable<T>> GetAllAsync();

        Task<T?> GetByIdAsync(object id);

        Task<IEnumerable<T>> FindByConditionAsync(Expression<Func<T, bool>> predicate);

        Task<T> AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task SaveChangesAsync();

        Task<bool> ExistsAsync(object id);

        Task<IEnumerable<T>> FindByConditionAsync(
       Expression<Func<T, bool>> predicate,
       bool trackChanges = false,
       params Expression<Func<T, object>>[] includes);

        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate); 

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);             

        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);    
         
        Task AddRangeAsync(IEnumerable<T> entities);

        Task UpdateRangeAsync(IEnumerable<T> entities);

        Task DeleteRangeAsync(IEnumerable<T> entities);

         Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);   
         
        Task<IEnumerable<T>> FindByConditionWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);
    }
} 
