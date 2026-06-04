using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.AI;

/// <summary>
/// Input features for profit maximization prediction per zone.
/// </summary>
public record ProfitMaximizationInput(
    [property: JsonPropertyName("target_datetime")] string TargetDatetime,
    [property: JsonPropertyName("zone_id")] int ZoneId,
    [property: JsonPropertyName("current_drivers")] int CurrentDrivers,
    [property: JsonPropertyName("allow_as_source")] bool AllowAsSource,
    [property: JsonPropertyName("allow_as_target")] bool AllowAsTarget,
    [property: JsonPropertyName("is_event_zone")] bool IsEventZone,
    [property: JsonPropertyName("is_airport_zone")] bool IsAirportZone,
    [property: JsonPropertyName("hour")] int Hour,
    [property: JsonPropertyName("day_of_week")] int DayOfWeek,
    [property: JsonPropertyName("is_weekend")] int IsWeekend,
    [property: JsonPropertyName("temp_c")] double TempC,
    [property: JsonPropertyName("rain_mm")] double RainMm,
    [property: JsonPropertyName("is_rain")] int IsRain,
    [property: JsonPropertyName("weather_code")] int WeatherCode,
    [property: JsonPropertyName("is_holiday")] int IsHoliday,
    [property: JsonPropertyName("lag_1_6h")] double Lag16h,
    [property: JsonPropertyName("lag_2_6h")] double Lag26h,
    [property: JsonPropertyName("lag_4_6h")] double Lag46h,
    [property: JsonPropertyName("rolling_mean_24h")] double RollingMean24h,
    [property: JsonPropertyName("rev_lag_1_6h")] double RevLag16h,
    [property: JsonPropertyName("rev_lag_1_week")] double RevLag1Week,
    [property: JsonPropertyName("rev_rolling_mean_7d")] double RevRollingMean7d,
    [property: JsonPropertyName("rev_rolling_mean_30d")] double RevRollingMean30d,
    [property: JsonPropertyName("avg_fare")] double AvgFare,
    [property: JsonPropertyName("tip_rate")] double TipRate,
    [property: JsonPropertyName("pickup_count")] int PickupCount,
    [property: JsonPropertyName("dropoff_count")] int DropoffCount,
    [property: JsonPropertyName("net_flow")] double NetFlow,
    [property: JsonPropertyName("activity_ratio")] double ActivityRatio,
    [property: JsonPropertyName("lag_1_pickup")] double Lag1Pickup,
    [property: JsonPropertyName("lag_1_dropoff")] double Lag1Dropoff,
    [property: JsonPropertyName("lag_1_net_flow")] double Lag1NetFlow
);

/// <summary>
/// Result of a profit maximization recommendation plan.
/// </summary>
public record ProfitMaximizationResult(
    [property: JsonPropertyName("question")] string Question,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("net_impact")] ProfitNetImpact NetImpact,
    [property: JsonPropertyName("kpis")] ProfitKpis Kpis,
    [property: JsonPropertyName("reposition_plan")] List<ProfitRepositionPlanItem> RepositionPlan,
    [property: JsonPropertyName("rejected_moves")] List<ProfitRejectedMoveItem> RejectedMoves,
    [property: JsonPropertyName("zone_evaluations")] List<ProfitZoneEvaluation> ZoneEvaluations
);

/// <summary>
/// Net financial and operational impact summary of the profit maximization plan.
/// </summary>
public record ProfitNetImpact(
    [property: JsonPropertyName("total_drivers_moved")] int TotalDriversMoved,
    [property: JsonPropertyName("deficit_resolved")] int DeficitResolved,
    [property: JsonPropertyName("total_move_cost")] double TotalMoveCost,
    [property: JsonPropertyName("expected_profit_uplift")] double ExpectedProfitUplift,
    [property: JsonPropertyName("total_baseline_profit")] double TotalBaselineProfit,
    [property: JsonPropertyName("total_projected_profit")] double TotalProjectedProfit,
    [property: JsonPropertyName("roi_percent")] double RoiPercent
);

/// <summary>
/// KPI metrics comparing states before and after optimization.
/// </summary>
public record ProfitKpis(
    [property: JsonPropertyName("target_deficit_before")] int TargetDeficitBefore,
    [property: JsonPropertyName("target_deficit_after")] int TargetDeficitAfter
);

/// <summary>
/// A recommended driver relocation assignment in the profit maximization plan.
/// </summary>
public record ProfitRepositionPlanItem(
    [property: JsonPropertyName("from_zone_id")] int FromZoneId,
    [property: JsonPropertyName("to_zone_id")] int ToZoneId,
    [property: JsonPropertyName("drivers_moved")] int DriversMoved,
    [property: JsonPropertyName("move_cost")] double MoveCost,
    [property: JsonPropertyName("expected_profit")] double ExpectedProfit,
    long? FromOsmId = null,
    double? FromCenterLatitude = null,
    double? FromCenterLongitude = null,
    long? ToOsmId = null,
    double? ToCenterLatitude = null,
    double? ToCenterLongitude = null
);

/// <summary>
/// Relocations evaluated but rejected during profit optimization.
/// </summary>
public record ProfitRejectedMoveItem(
    [property: JsonPropertyName("from_zone_id")] int FromZoneId,
    [property: JsonPropertyName("to_zone_id")] int ToZoneId,
    [property: JsonPropertyName("expected_profit")] double ExpectedProfit,
    [property: JsonPropertyName("move_cost")] double MoveCost,
    [property: JsonPropertyName("reason")] string Reason,
    long? FromOsmId = null,
    double? FromCenterLatitude = null,
    double? FromCenterLongitude = null,
    long? ToOsmId = null,
    double? ToCenterLatitude = null,
    double? ToCenterLongitude = null
);

/// <summary>
/// Detailed prediction metrics and optimization suitability evaluated per zone.
/// </summary>
public record ProfitZoneEvaluation(
    [property: JsonPropertyName("zone_id")] int ZoneId,
    [property: JsonPropertyName("current_drivers")] int CurrentDrivers,
    [property: JsonPropertyName("allow_as_source")] bool AllowAsSource,
    [property: JsonPropertyName("allow_as_target")] bool AllowAsTarget,
    [property: JsonPropertyName("is_airport_zone")] bool IsAirportZone,
    [property: JsonPropertyName("demand_6h")] double Demand6h,
    [property: JsonPropertyName("cycle_time_min")] double CycleTimeMin,
    [property: JsonPropertyName("trips_per_driver_6h")] double TripsPerDriver6h,
    [property: JsonPropertyName("drivers_needed_6h")] int DriversNeeded6h,
    [property: JsonPropertyName("driver_gap")] int DriverGap,
    [property: JsonPropertyName("deficit")] int Deficit,
    [property: JsonPropertyName("surplus")] int Surplus,
    [property: JsonPropertyName("revenue_p50")] double RevenueP50,
    [property: JsonPropertyName("revenue_p90")] double RevenueP90,
    [property: JsonPropertyName("uncertainty")] double Uncertainty,
    [property: JsonPropertyName("stockout_prob")] double StockoutProb,
    [property: JsonPropertyName("served_ratio_baseline")] double ServedRatioBaseline,
    [property: JsonPropertyName("baseline_profit")] double BaselineProfit,
    [property: JsonPropertyName("source_candidate")] bool SourceCandidate,
    [property: JsonPropertyName("target_candidate")] bool TargetCandidate,
    [property: JsonPropertyName("reason")] string Reason,
    long? OsmId = null,
    double? CenterLatitude = null,
    double? CenterLongitude = null
);
