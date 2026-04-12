
using NYCTaxiData.Domain.Common.Interfaces;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User1> Users { get; }

        IGenericRepository<Manager> Managers { get; }

        IGenericRepository<Driver> Drivers { get; }

        IGenericRepository<Trip> Trips { get; }

        IGenericRepository<Zone> Zones { get; }

        IGenericRepository<Location> Locations { get; }

        IGenericRepository<Simulationrequest> SimulationRequests { get; }

        IGenericRepository<Simulationresult> SimulationResults { get; }

        IGenericRepository<Demandprediction> DemandPredictions { get; }

        IGenericRepository<Weathersnapshot> WeatherSnapshots { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default);
    }
}
