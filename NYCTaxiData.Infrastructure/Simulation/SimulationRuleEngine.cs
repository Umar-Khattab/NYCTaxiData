using Microsoft.Extensions.Options;
using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Application.Simulation.Models;

namespace NYCTaxiData.Infrastructure.Simulation;

public sealed class SimulationRuleEngine : ISimulationRuleEngine
{
    private readonly SimulationOptions _options;

    public SimulationRuleEngine(IOptions<SimulationOptions> options)
    {
        _options = options.Value;
    }

    public IReadOnlyList<DriverRelocation> ComputeRelocations(SimulationState state)
    {
        var relocations = new List<DriverRelocation>();
        var deficits = new PriorityQueue<int, double>();
        var surpluses = new PriorityQueue<int, double>();

        foreach (var zone in state.Zones.Values)
        {
            var availableDrivers = state.Drivers.Values.Count(d => d.ZoneId == zone.ZoneId && d.Status == DriverStatus.Available);
            var gap = zone.Demand - availableDrivers;
            if (gap > 1)
            {
                deficits.Enqueue(zone.ZoneId, -gap);
            }
            else if (gap < -1)
            {
                surpluses.Enqueue(zone.ZoneId, gap);
            }
        }

        var movesRemaining = _options.MaxRelocationsPerHour;
        while (movesRemaining > 0 && deficits.Count > 0 && surpluses.Count > 0)
        {
            var toZone = deficits.Dequeue();
            var fromZone = surpluses.Dequeue();
            var driversToMove = state.Drivers.Values
                .Where(driver => driver.ZoneId == fromZone && driver.Status == DriverStatus.Available)
                .Take(movesRemaining)
                .ToList();

            foreach (var driver in driversToMove)
            {
                relocations.Add(new DriverRelocation(driver.DriverId, fromZone, toZone));
                movesRemaining--;
                if (movesRemaining == 0)
                {
                    break;
                }
            }
        }

        return relocations;
    }
}
