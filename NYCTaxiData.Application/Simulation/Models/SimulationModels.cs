using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.DTOs.Simulation;

namespace NYCTaxiData.Application.Simulation.Models;

public sealed class SimulationState
{
    public Guid SimulationId { get; init; } = Guid.NewGuid();
    public DateTime StartTime { get; init; }
    public DateTime CurrentTime { get; set; }
    public int CurrentHourIndex { get; set; }
    public int DurationHours { get; init; }
    public SimulationStatus Status { get; set; } = SimulationStatus.Queued;
    public bool IsPaused { get; set; }
    public double SpeedFactor { get; set; }
    public Dictionary<int, ZoneState> Zones { get; } = new();
    public Dictionary<int, DriverState> Drivers { get; } = new();
    public PriorityQueue<TripState, DateTime> ActiveTrips { get; } = new();
}

public sealed class ZoneState
{
    public int ZoneId { get; init; }
    public int DriverCount { get; set; }
    public int ActiveTrips { get; set; }
    public double Demand { get; set; }
    public double EtaMinutes { get; set; }
    public double Revenue { get; set; }
    public double StockoutRisk { get; set; }
    public List<ZoneMetricPoint> History { get; } = new();
}

public sealed class DriverState
{
    public int DriverId { get; init; }
    public int ZoneId { get; set; }
    public DriverStatus Status { get; set; } = DriverStatus.Available;
}

public enum DriverStatus
{
    Available,
    OnTrip,
    Relocating
}

public sealed class TripState
{
    public int TripId { get; init; }
    public int DriverId { get; init; }
    public int PickupZoneId { get; init; }
    public int DropoffZoneId { get; init; }
    public DateTime EndTime { get; init; }
}

public sealed class SimulationZoneFeatures
{
    public int ZoneId { get; init; }
    public Demand6hInput DemandInput { get; init; } = default!;
    public RevenueInput RevenueInput { get; init; } = default!;
    public StockOutInput StockOutInput { get; init; } = default!;
    public ETAInput EtaInput { get; init; } = default!;
}

public sealed class SimulationPredictionSet
{
    public Dictionary<int, double> DemandByZone { get; } = new();
    public Dictionary<int, double> EtaMinutesByZone { get; } = new();
    public Dictionary<int, double> RevenueByZone { get; } = new();
    public Dictionary<int, double> StockoutRiskByZone { get; } = new();
}
