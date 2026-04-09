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

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await action();
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }

}