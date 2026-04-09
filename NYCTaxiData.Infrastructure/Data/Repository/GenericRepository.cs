using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Domain.Common.Interfaces;
using NYCTaxiData.Infrastructure.Data.Contexts;
using System.Linq.Expressions;

namespace NYCTaxiData.Infrastructure.Data.Repository
{

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly TaxiDbContext _context;

        protected readonly DbSet<T> _dbSet;

        public GenericRepository(TaxiDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        public async Task<T?> GetByIdAsync(object id)
            => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> FindByConditionAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public async Task<IEnumerable<T>> FindByConditionAsync(
            Expression<Func<T, bool>> predicate,
            bool trackChanges = false,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = trackChanges ? _dbSet : _dbSet.AsNoTracking();
            query = query.Where(predicate);
            query = includes.Aggregate(query, (current, include) => current.Include(include));
            return await query.ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public async Task<bool> ExistsAsync(object id)
            => await _dbSet.FindAsync(id) != null;

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.FirstOrDefaultAsync(predicate);

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
            => predicate == null
                ? await _dbSet.CountAsync()
                : await _dbSet.CountAsync(predicate);

        public async Task AddRangeAsync(IEnumerable<T> entities)
            => await _dbSet.AddRangeAsync(entities);

        public Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            if (predicate != null)
                query = query.Where(predicate);

            var totalCount = await query.CountAsync();

            if (orderBy != null)
                query = orderBy(query);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<T>> FindByConditionWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsNoTracking().Where(predicate);
            query = includes.Aggregate(query, (current, include) => current.Include(include));
            return await query.ToListAsync();
        }
    }
}