using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Domain.DTOs;

namespace NYCTaxiData.Infrastructure.Simulation;

public sealed class SimulationResultStore : ISimulationResultStore
{
    private readonly object _sync = new();
    private readonly List<SimulationTick> _ticks = new();
    private readonly Dictionary<int, List<ZoneMetricPoint>> _zoneHistory = new();
    private Guid _simulationId = Guid.Empty;

    public void Reset(Guid simulationId)
    {
        lock (_sync)
        {
            _simulationId = simulationId;
            _ticks.Clear();
            _zoneHistory.Clear();
        }
    }

    public void AppendTick(SimulationTick tick)
    {
        lock (_sync)
        {
            _ticks.Add(tick);
            foreach (var zone in tick.Zones)
            {
                if (!_zoneHistory.TryGetValue(zone.ZoneId, out var history))
                {
                    history = new List<ZoneMetricPoint>();
                    _zoneHistory[zone.ZoneId] = history;
                }

                history.Add(new ZoneMetricPoint(
                    tick.SimulatedTime,
                    zone.Demand,
                    zone.Revenue,
                    zone.EtaMinutes,
                    zone.StockoutRisk,
                    zone.DriverCount,
                    zone.ActiveTrips));
            }
        }
    }

    public SimulationTick? GetLatestTick()
    {
        lock (_sync)
        {
            return _ticks.Count == 0 ? null : _ticks[^1];
        }
    }

    public ZoneHistoryResponse GetZoneHistory(int zoneId)
    {
        lock (_sync)
        {
            return new ZoneHistoryResponse(
                zoneId,
                _zoneHistory.TryGetValue(zoneId, out var history) ? history.ToList() : new List<ZoneMetricPoint>());
        }
    }

    public SimulationPlaybackChunk GetPlayback(int startHour, int endHour)
    {
        lock (_sync)
        {
            var ticks = _ticks
                .Where(tick => tick.HourIndex >= startHour && tick.HourIndex <= endHour)
                .ToList();

            return new SimulationPlaybackChunk(_simulationId.ToString(), ticks);
        }
    }

    public IReadOnlyList<int> GetZoneIds()
    {
        lock (_sync)
        {
            return _zoneHistory.Keys.OrderBy(id => id).ToList();
        }
    }
}
