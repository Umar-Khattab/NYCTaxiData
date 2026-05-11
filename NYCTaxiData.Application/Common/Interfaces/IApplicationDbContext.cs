using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        IQueryable<User1> Users1 { get; }

        Task<User1?> GetUserByPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
