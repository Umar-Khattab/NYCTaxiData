using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NYCTaxiData.Domain.Enums;

namespace NYCTaxiData.Application.DTOs.Simulation;

/// <summary>
/// Request to start a faster-than-real-time simulation run.
/// </summary>
public record SimulationStartRequest(
    [Range(1, 720)] int DurationHours,
    [Range(1, 200)] double SpeedFactor,
    [Range(1, 10000)] int TotalDrivers,
    [Range(1, 265)] int ZoneCount,
    DateTime StartTime
);

/// <summary>
/// Controls an in-flight simulation session.
/// </summary>
public record SimulationControlRequest(
    string Action,
    [Range(1, 200)] double? SpeedFactor = null
);

/// <summary>
/// Current status snapshot for a simulation session.
/// </summary>
public record SimulationStatusResponse(
    string SimulationId,
    SimulationStatus Status,
    DateTime? SimulatedTime,
    int CurrentHour,
    double SpeedFactor,
    bool IsPaused
);

/// <summary>
/// Aggregate metrics for a simulation timestep.
/// </summary>
public record SimulationAggregateMetrics(
    double TotalDemand,
    double TotalRevenue,
    double AvgEtaMinutes,
    double AvgStockoutRisk,
    int TotalDrivers,
    int TotalActiveTrips
);

/// <summary>
/// Per-zone metrics at a simulation timestep.
/// </summary>
public record ZoneSimulationSnapshot(
    int ZoneId,
    int DriverCount,
    int ActiveTrips,
    double Demand,
    double EtaMinutes,
    double Revenue,
    double StockoutRisk
);

/// <summary>
/// Emitted tick payload for WebSocket clients.
/// </summary>
public record SimulationTick(
    string SimulationId,
    DateTime SimulatedTime,
    int HourIndex,
    SimulationAggregateMetrics Aggregate,
    IReadOnlyList<ZoneSimulationSnapshot> Zones
);

/// <summary>
/// Single zone metrics history point.
/// </summary>
public record ZoneMetricPoint(
    DateTime SimulatedTime,
    double Demand,
    double Revenue,
    double EtaMinutes,
    double StockoutRisk,
    int DriverCount,
    int ActiveTrips
);

/// <summary>
/// Response for zone history queries.
/// </summary>
public record ZoneHistoryResponse(
    int ZoneId,
    IReadOnlyList<ZoneMetricPoint> Points
);

/// <summary>
/// Response for playback scrubbing.
/// </summary>
public record SimulationPlaybackChunk(
    string SimulationId,
    IReadOnlyList<SimulationTick> Ticks
);
