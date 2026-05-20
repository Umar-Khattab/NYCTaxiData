using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage; 
using NYCTaxiData.Domain.Common.Interfaces;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Data.Contexts;
using NYCTaxiData.Infrastructure.Data.Repository; 

namespace NYCTaxiData.Infrastructure.Services;

public class UnitOfWork :  IUnitOfWork
{
    private readonly TaxiDbContext _context;
    private IDbContextTransaction? _transaction;
    private IGenericRepository<User1>? _users;
    private IGenericRepository<Manager>? _managers;
    private IGenericRepository<Driver>? _drivers;
    private IGenericRepository<Trip>? _trips;
    private IGenericRepository<Zone>? _zones;
    private IGenericRepository<Location>? _locations; 

    public UnitOfWork(TaxiDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<User1> Users
        => _users ??= new GenericRepository<User1>(_context);

    public IGenericRepository<Manager> Managers
        => _managers ??= new GenericRepository<Manager>(_context);

    public IGenericRepository<Driver> Drivers
        => _drivers ??= new GenericRepository<Driver>(_context);

    public IGenericRepository<Trip> Trips
        => _trips ??= new GenericRepository<Trip>(_context);

    public IGenericRepository<Zone> Zones
        => _zones ??= new GenericRepository<Zone>(_context);

    public IGenericRepository<Location> Locations
        => _locations ??= new GenericRepository<Location>(_context); 

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken ct)
    {
        // 1. تشيك لو فيه Transaction شغالة حالياً
        if (_context.Database.CurrentTransaction != null)
        {
            // لو فيه واحدة، نفذ العملية فوراً من غير ما تفتح واحدة جديدة
            return await operation(ct);
        }

        // 2. لو مفيش، افتح واحدة جديدة باستخدام الاستراتيجية
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var result = await operation(ct);
                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return result;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }
}