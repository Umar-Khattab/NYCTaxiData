using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Domain.Common.Interfaces;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Data.Contexts;
using NYCTaxiData.Infrastructure.Data.Repository;

namespace NYCTaxiData.Infrastructure.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly TaxiDbContext _context;
    private IDbContextTransaction? _transaction;
    private IGenericRepository<User1>? _users;
    private IGenericRepository<Manager>? _managers;
    private IGenericRepository<Driver>? _drivers;
    private IGenericRepository<Trip>? _trips;
    private IGenericRepository<Zone>? _zones;
    private IGenericRepository<Location>? _locations;
    private IGenericRepository<Simulationrequest>? _simulationRequests;
    private IGenericRepository<Simulationresult>? _simulationResults;
    private IGenericRepository<Demandprediction>? _demandPredictions;
    private IGenericRepository<Weathersnapshot>? _weatherSnapshots;

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

    public IGenericRepository<Simulationrequest> SimulationRequests
        => _simulationRequests ??= new GenericRepository<Simulationrequest>(_context);

    public IGenericRepository<Simulationresult> SimulationResults
        => _simulationResults ??= new GenericRepository<Simulationresult>(_context);

    public IGenericRepository<Demandprediction> DemandPredictions
        => _demandPredictions ??= new GenericRepository<Demandprediction>(_context);

    public IGenericRepository<Weathersnapshot> WeatherSnapshots
        => _weatherSnapshots ??= new GenericRepository<Weathersnapshot>(_context);

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
    // داخل UnitOfWork.cs
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken ct)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            // 👈 السر هنا: await using بدلاً من using العادية
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var result = await operation(ct);
                await transaction.CommitAsync(ct);
                return result;
            }
            catch { await transaction.RollbackAsync(ct); throw; }
        });
    }
}