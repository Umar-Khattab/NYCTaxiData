using NYCTaxiData.Application.Common.Models;

namespace NYCTaxiData.API.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var totalCount = query.Count();

            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return PaginatedList<T>.Create(items, totalCount, pageNumber, pageSize);
        }

        public static PaginatedList<T> ToPaginatedList<T>(
            this IEnumerable<T> source,
            int pageNumber,
            int pageSize)
        {
            var list = source.ToList();
            var totalCount = list.Count;
            var items = list
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return PaginatedList<T>.Create(items, totalCount, pageNumber, pageSize);
        }
    }
}
