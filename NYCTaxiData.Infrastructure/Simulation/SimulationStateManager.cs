using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Application.Simulation.Models;
using NYCTaxiData.Application.DTOs.Simulation;
using NYCTaxiData.Domain.Enums;

namespace NYCTaxiData.Infrastructure.Simulation;

public sealed class SimulationStateManager : ISimulationStateManager
{
    private int _tripIdSeed = 1;

    public SimulationState InitializeState(SimulationStartRequest request, IReadOnlyList<SimulationZoneFeatures> features)
    {
        var state = new SimulationState
        {
            SimulationId = Guid.NewGuid(),
            StartTime = request.StartTime,
            CurrentTime = request.StartTime,
            DurationHours = request.DurationHours,
            SpeedFactor = request.SpeedFactor,
            Status = SimulationStatus.Queued,
            CurrentHourIndex = 0
        };

        if (features.Count == 0)
        {
            for (var zoneId = 1; zoneId <= request.ZoneCount; zoneId++)
            {
                state.Zones[zoneId] = new ZoneState
                {
                    ZoneId = zoneId,
                    Demand = 0,
                    EtaMinutes = 0,
                    Revenue = 0,
                    StockoutRisk = 0
                };
            }
        }
        else
        {
            foreach (var feature in features)
            {
                state.Zones[feature.ZoneId] = new ZoneState
                {
                    ZoneId = feature.ZoneId,
                    Demand = 0,
                    EtaMinutes = 0,
                    Revenue = 0,
                    StockoutRisk = 0
                };
            }
        }

        var zoneIds = state.Zones.Keys.OrderBy(id => id).ToArray();
        var driversPerZone = request.TotalDrivers / zoneIds.Length;
        var remainder = request.TotalDrivers % zoneIds.Length;
        var driverId = 1;

        for (var index = 0; index < zoneIds.Length; index++)
        {
            var zoneId = zoneIds[index];
            var count = driversPerZone + (index < remainder ? 1 : 0);
            for (var i = 0; i < count; i++)
            {
                state.Drivers[driverId] = new DriverState
                {
                    DriverId = driverId,
                    ZoneId = zoneId,
                    Status = DriverStatus.Available
                };
                driverId++;
            }
        }

        RefreshZoneStats(state);
        return state;
    }

    public void ApplyStep(SimulationState state, SimulationPredictionSet predictions)
    {
        ReleaseCompletedTrips(state);

        foreach (var zone in state.Zones.Values)
        {
            zone.Demand = predictions.DemandByZone.GetValueOrDefault(zone.ZoneId, zone.Demand);
            zone.EtaMinutes = predictions.EtaMinutesByZone.GetValueOrDefault(zone.ZoneId, zone.EtaMinutes);
            zone.Revenue = predictions.RevenueByZone.GetValueOrDefault(zone.ZoneId, zone.Revenue);
            zone.StockoutRisk = predictions.StockoutRiskByZone.GetValueOrDefault(zone.ZoneId, zone.StockoutRisk);
        }

        StartTrips(state);
        RefreshZoneStats(state);

    }

    public void ApplyRelocations(SimulationState state, IReadOnlyList<DriverRelocation> relocations)
    {
        foreach (var relocation in relocations)
        {
            if (state.Drivers.TryGetValue(relocation.DriverId, out var driver) &&
                driver.Status == DriverStatus.Available)
            {
                driver.ZoneId = relocation.ToZoneId;
                driver.Status = DriverStatus.Relocating;
            }
        }

        foreach (var relocation in relocations)
        {
            if (state.Drivers.TryGetValue(relocation.DriverId, out var driver))
            {
                driver.Status = DriverStatus.Available;
            }
        }

        RefreshZoneStats(state);
    }

    public SimulationTick BuildTick(SimulationState state)
        => BuildTickInternal(state);

    private void ReleaseCompletedTrips(SimulationState state)
    {
        while (state.ActiveTrips.TryPeek(out _, out var endTime) && endTime <= state.CurrentTime)
        {
            var trip = state.ActiveTrips.Dequeue();
            if (state.Drivers.TryGetValue(trip.DriverId, out var driver))
            {
                driver.Status = DriverStatus.Available;
                driver.ZoneId = trip.DropoffZoneId;
            }
        }
    }

    private void StartTrips(SimulationState state)
    {
        foreach (var zone in state.Zones.Values)
        {
            var availableDrivers = state.Drivers.Values
                .Where(driver => driver.ZoneId == zone.ZoneId && driver.Status == DriverStatus.Available)
                .ToList();

            var tripsToStart = (int)Math.Min(availableDrivers.Count, Math.Round(zone.Demand));
            for (var i = 0; i < tripsToStart; i++)
            {
                var driver = availableDrivers[i];
                driver.Status = DriverStatus.OnTrip;
                var dropoffZone = zone.ZoneId == state.Zones.Count ? 1 : zone.ZoneId + 1;
                var etaMinutes = Math.Max(5, zone.EtaMinutes);
                var endTime = state.CurrentTime.AddMinutes(etaMinutes);

                var trip = new TripState
                {
                    TripId = _tripIdSeed++,
                    DriverId = driver.DriverId,
                    PickupZoneId = zone.ZoneId,
                    DropoffZoneId = dropoffZone,
                    EndTime = endTime
                };

                state.ActiveTrips.Enqueue(trip, endTime);
            }
        }
    }

    private static void RefreshZoneStats(SimulationState state)
    {
        foreach (var zone in state.Zones.Values)
        {
            zone.DriverCount = state.Drivers.Values.Count(driver => driver.ZoneId == zone.ZoneId);
            zone.ActiveTrips = state.Drivers.Values.Count(driver => driver.ZoneId == zone.ZoneId && driver.Status == DriverStatus.OnTrip);
        }
    }

    private static SimulationTick BuildTickInternal(SimulationState state)
    {
        foreach (var zone in state.Zones.Values)
        {
            zone.History.Add(new ZoneMetricPoint(
                state.CurrentTime,
                zone.Demand,
                zone.Revenue,
                zone.EtaMinutes,
                zone.StockoutRisk,
                zone.DriverCount,
                zone.ActiveTrips));
        }

        var zones = state.Zones.Values
            .Select(zone => new ZoneSimulationSnapshot(
                zone.ZoneId,
                zone.DriverCount,
                zone.ActiveTrips,
                Math.Round(zone.Demand, 2),
                Math.Round(zone.EtaMinutes, 2),
                Math.Round(zone.Revenue, 2),
                Math.Round(zone.StockoutRisk, 3)))
            .OrderBy(zone => zone.ZoneId)
            .ToList();

        var aggregate = new SimulationAggregateMetrics(
            zones.Sum(zone => zone.Demand),
            zones.Sum(zone => zone.Revenue),
            zones.Count == 0 ? 0 : zones.Average(zone => zone.EtaMinutes),
            zones.Count == 0 ? 0 : zones.Average(zone => zone.StockoutRisk),
            zones.Sum(zone => zone.DriverCount),
            zones.Sum(zone => zone.ActiveTrips));

        return new SimulationTick(
            state.SimulationId.ToString(),
            state.CurrentTime,
            state.CurrentHourIndex,
            aggregate,
            zones);
    }
}
