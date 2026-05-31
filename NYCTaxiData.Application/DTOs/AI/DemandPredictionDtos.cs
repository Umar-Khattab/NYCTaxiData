using NYCTaxiData.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.AI;

/// <summary>
/// Input features for 15-minute demand prediction per zone.
/// </summary>
public record Demand15MinInput(
    [property: JsonPropertyName("PULocationID")] int ZoneId,
    [property: JsonPropertyName("hour")] int Hour,
    [property: JsonPropertyName("minute")] int Minute,
    [property: JsonPropertyName("day_of_week")] int DayOfWeek,
    [property: JsonPropertyName("month")] int Month,
    [property: JsonPropertyName("is_weekend")] bool IsWeekend,
    [property: JsonPropertyName("lag_1")] double Lag1,
    [property: JsonPropertyName("lag_4")] double Lag4,
    [property: JsonPropertyName("lag_96")] double Lag96,
    [property: JsonPropertyName("roll_mean_1h")] double RollMean1h,
    [property: JsonPropertyName("roll_mean_3h")] double RollMean3h,
    [property: JsonPropertyName("temp_c")] double TempC,
    [property: JsonPropertyName("rain_mm")] double RainMm,
    [property: JsonPropertyName("is_rain")] bool IsRain,
    [property: JsonPropertyName("weather_code")] int WeatherCode,
    [property: JsonPropertyName("pickup_cnt")] int PickupCount
);
/// <summary>
/// Result of a 15-minute demand prediction for a single zone.
/// </summary>
public record Demand15MinResult(
    [property: JsonPropertyName("zone_id")] int ZoneId,
    [property: JsonPropertyName("predicted_demand")] double PredictedDemand,
    [property: JsonPropertyName("lower_bound")] double LowerBound,
    [property: JsonPropertyName("upper_bound")] double UpperBound
);

/// <summary>
/// Input features for 6-hour demand prediction per zone.
/// </summary>
public record Demand6hInput(
    [property: JsonPropertyName("PULocationID")] int ZoneId,
    [property: JsonPropertyName("pickup_hour")] int PickupHour,
    [property: JsonPropertyName("day_of_week")] int DayOfWeek,
    [property: JsonPropertyName("is_weekend")] bool IsWeekend,
    [property: JsonPropertyName("is_holiday")] bool IsHoliday,
    [property: JsonPropertyName("lag_1_6h")] double Lag1_6h,
    [property: JsonPropertyName("lag_2_6h")] double Lag2_6h,
    [property: JsonPropertyName("lag_4_6h")] double Lag4_6h,
    [property: JsonPropertyName("rolling_mean_24h")] double RollingMean24h,
    [property: JsonPropertyName("temp_c")] double TempC,
    [property: JsonPropertyName("rain_mm")] double RainMm,
    [property: JsonPropertyName("is_rain")] bool IsRain,
    [property: JsonPropertyName("weather_code")] int WeatherCode,
    [property: JsonPropertyName("pickup_count")] int PickupCount
);

/// <summary>
/// Result of a 6-hour demand prediction for a single zone.
/// </summary>
public record Demand6hResult(
    [property: JsonPropertyName("zone_id")] int ZoneId,
    [property: JsonPropertyName("predicted_demand")] double PredictedDemand,
    [property: JsonPropertyName("confidence_interval_lower")] double ConfidenceIntervalLower,
    [property: JsonPropertyName("confidence_interval_upper")] double ConfidenceIntervalUpper
);

/// <summary>
/// Input features for ETA prediction between two zones.
/// </summary>
public record ETAInput(
    [property: JsonPropertyName("PULocationID")] int PickupZoneId,
    [property: JsonPropertyName("DOLocationID")] int DropoffZoneId,
    [property: JsonPropertyName("temp_c")] decimal? TempC,
    [property: JsonPropertyName("rain_mm")] decimal? RainMm,
    [property: JsonPropertyName("weather_code")] int? WeatherCode,
    [property: JsonPropertyName("trip_distance")] decimal DistanceProxy, // تم التغيير من distance_proxy لـ trip_distance
    [property: JsonPropertyName("pickup_hour")] int PUHour,
    [property: JsonPropertyName("pickup_dow")] int PUDow,
    [property: JsonPropertyName("pickup_month")] int PUMonth,
    [property: JsonPropertyName("pickup_minute")] int PUMinute,
    [property: JsonPropertyName("is_weekend")] int IsWeekend,
    [property: JsonPropertyName("is_rush_hour")] int IsRushHour,
    [property: JsonPropertyName("pickup_datetime")] DateTime PU15MinBucket, // تم التغيير من pickup_15min_bucket لـ pickup_datetime
    [property: JsonPropertyName("distance_bucket_label")] string DistanceBucketLabel,
    [property: JsonPropertyName("duration_sec")] decimal DurationSec,
    [property: JsonPropertyName("od_hour_median_duration")] decimal OdHourMedianDuration,
    [property: JsonPropertyName("pu_hour_slowdown_index")] decimal PUHourSlowdownIndex,
    [property: JsonPropertyName("dist_median_duration")] int DistMedianDuration
);

/// <summary>
/// Result of an ETA prediction for a zone pair.
/// </summary>
/// public record PredictionResponse(

public record ETAResult(
    [property: JsonPropertyName("pu_location_id")] int PickupZoneId,
    [property: JsonPropertyName("do_location_id")] int DropoffZoneId,
    [property: JsonPropertyName("p50_seconds")] double P50Seconds,
    [property: JsonPropertyName("p90_seconds")] double P90Seconds
)
{

    [JsonIgnore]
    public double P50Minutes => P50Seconds / 60.0;

    [JsonIgnore]
    public double P90Minutes => P90Seconds / 60.0;
}
/// <summary>
/// Input features for revenue prediction per zone.
/// </summary> 
public record RevenueInput(
    [property: JsonPropertyName("PULocationID")] int ZoneId,
    [property: JsonPropertyName("pickup_hour")] int PickupHour,
    [property: JsonPropertyName("day_of_week")] int DayOfWeek,
    [property: JsonPropertyName("is_weekend")] bool IsWeekend,
    [property: JsonPropertyName("lag_1_6h")] int lag1_6h, // غير الحرف الأول لصغير
    [property: JsonPropertyName("lag_2_6h")] int lag2_6h,
    [property: JsonPropertyName("lag_4_6h")] int lag4_6h,
    [property: JsonPropertyName("rev_lag_1_6h")] double RevLag1_6h,
    [property: JsonPropertyName("rev_lag_1_week")] double RevLag1Week,
    [property: JsonPropertyName("rev_rolling_mean_7d")] double RevRollingMean7d,
    [property: JsonPropertyName("rev_rolling_mean_30d")] double RevRollingMean30d,
    [property: JsonPropertyName("rolling_mean_24h")] decimal? RollingMean24h,
    [property: JsonPropertyName("avg_fare")] double AvgFare,
    [property: JsonPropertyName("tip_rate")] double TipRate,
    [property: JsonPropertyName("temp_c")] double? TempC,
    [property: JsonPropertyName("rain_mm")] double? RainMm,
    [property: JsonPropertyName("is_rain")] bool? IsRain,
    [property: JsonPropertyName("weather_code")] int? WeatherCode,
    [property: JsonPropertyName("is_holiday")] bool? IsHoliday
);

/// <summary>
/// Result of a revenue prediction for a single zone.
/// </summary>
public record RevenueResult(
    [property: JsonPropertyName("zone_id")] int ZoneId,
    [property: JsonPropertyName("p50")] double P50,
    [property: JsonPropertyName("p90")] double P90
);

/// <summary>
/// Input features for stock-out prediction per zone.
/// </summary>
public record StockOutInput(
    [Range(1, 265)] int ZoneId,
    DateTime TimeBucket6h,
    double PickupCount,
    double DropoffCount,
    double NetFlow,
    [Range(0, 23)] int Hour,
    [Range(0, 6)] int DayOfWeek,
    bool IsWeekend,
    bool IsHoliday,
    double ActivityRatio,
    double TempC,
    [Range(0, double.MaxValue)] double RainMm,
    bool IsRain, 
    double Lag1Pickup,
    double Lag1Dropoff,
    double Lag1NetFlow,
    int WeatherCode
);

/// <summary>
/// Result of a stock-out prediction for a single zone.
/// </summary>
public record StockOutResult(
    [property: JsonPropertyName("zone_id")] int ZoneId,
    [property: JsonPropertyName("probability")] double Probability
)
{
    /// <summary>
    /// Indicates whether the zone is predicted to stock out.
    /// </summary>
    public bool WillStockOut => Probability > 0.5;

    /// <summary>
    /// The assessed risk level based on the stock-out probability.
    /// </summary>
    public RiskLevel RiskLevel => Probability switch
    {
        > 0.8 => RiskLevel.Critical,
        > 0.5 => RiskLevel.High,
        > 0.3 => RiskLevel.Medium,
        _ => RiskLevel.Low
    };
}

/// <summary>
/// Result of a profit zone ranking analysis.
/// </summary>
public record ProfitZoneResult(
    int ZoneId,
    int Rank,
    double ProfitScore,
    double ExpectedRevenue,
    double ExpectedCost,
    [Range(0, 1)] double StockOutRisk,
    double ExpectedDemand,
    double AvgETAFromCurrentLocation
)
{
    /// <summary>
    /// Net profit after subtracting expected costs from expected revenue.
    /// </summary>
    public double NetProfit => ExpectedRevenue - ExpectedCost;
}

/// <summary>
/// Result of a causal impact estimation for a treatment event in a zone.
/// </summary>
public record CausalImpactResult(
    int ZoneId,
    string TreatmentType,
    double BaselineDemand,
    double ActualDemand,
    double DemandUplift,
    double ConfidenceIntervalLower,
    double ConfidenceIntervalUpper,
    double PValue,
    string Interpretation
)
{
    /// <summary>
    /// Percentage uplift relative to the baseline demand.
    /// </summary>
    public double UpliftPercentage => BaselineDemand > 0 ? (DemandUplift / BaselineDemand) * 100 : 0;

    /// <summary>
    /// Indicates whether the causal effect is statistically significant (p &lt; 0.05).
    /// </summary>
    public bool IsSignificant => PValue < 0.05;
}

/// <summary>
/// Represents the current supply state of a single zone for optimization.
/// </summary>
public record ZoneSupplyState(
    [Range(1, 265)] int ZoneId,
    [Range(0, int.MaxValue)] int CurrentSupply,
    [Range(0, int.MaxValue)] int ActiveTrips,
    double? ForecastedDemand,
    [Range(0, 1)] double? StockOutRisk,
    double? ExpectedRevenue
);

/// <summary>
/// Constraints applied to the repositioning optimization algorithm.
/// </summary>
public record OptimizationConstraints(
    [Range(1, int.MaxValue)] int MaxTravelTimeMinutes = 30,
    [Range(0, double.MaxValue)] double MinProfitPerTrip = 15.0,
    [Range(0, 1)] double MaxEmptyRelocationRatio = 0.3,
    [Range(1, int.MaxValue)] int? MaxVehiclesToMove = null
);

/// <summary>
/// A single vehicle repositioning assignment from one zone to another.
/// </summary>
public record RepositionAssignment(
    [property: JsonPropertyName("assignment_id")] string AssignmentId,
    [property: JsonPropertyName("from_zone_id")] int FromZoneId,
    [property: JsonPropertyName("to_zone_id")] int ToZoneId,
    [property: JsonPropertyName("vehicle_count")] int VehicleCount,
    [property: JsonPropertyName("eta_minutes")] double ETAMinutes,
    [property: JsonPropertyName("estimated_cost")] double EstimatedCost,
    [property: JsonPropertyName("expected_profit")] double ExpectedProfit
);

/// <summary>
/// Summary of supply and demand for a zone after optimization.
/// </summary>
public record ZonePlanSummary(
    [property: JsonPropertyName("zone_id")] int ZoneId,
    [property: JsonPropertyName("supply_before")] int SupplyBefore,
    [property: JsonPropertyName("supply_after")] int SupplyAfter,
    [property: JsonPropertyName("demand_forecast")] double DemandForecast,
    [property: JsonPropertyName("coverage_ratio_after")] double CoverageRatioAfter
);

/// <summary>
/// Core operational metrics from a simulation run.
/// </summary>
public record SimulationMetrics(
    [Range(0, 1)] double DemandCoverage,
    double AvgWaitTimeMinutes,
    [Range(0, 1)] double StockOutRate,
    double TotalRevenue,
    double TotalOperationalCost,
    double NetProfit,
    int TotalVehicles
);

/// <summary>
/// Financial impact analysis comparing baseline to simulated scenarios.
/// </summary>
public record FinancialImpact(
    double RevenueIncrease,
    double AdditionalOperationalCost,
    double NetProfitImpact,
    double? PaybackPeriodMonths
)
{
    /// <summary>
    /// Return on investment as a percentage.
    /// </summary>
    public double ROI => AdditionalOperationalCost > 0
        ? (RevenueIncrease - AdditionalOperationalCost) / AdditionalOperationalCost * 100
        : 0;
}

/// <summary>
/// Per-zone breakdown of a simulation result.
/// </summary>
public record ZoneSimulationResult(
    int ZoneId,
    int AdditionalVehicles,
    [Range(0, 1)] double DemandCoverageBefore,
    [Range(0, 1)] double DemandCoverageAfter,
    double RevenueImpact,
    [Range(0, 1)] double StockOutRiskBefore,
    [Range(0, 1)] double StockOutRiskAfter
);

/// <summary>
/// A recommendation derived from simulation analysis.
/// </summary>
public record SimulationRecommendation(
    RecommendationType Type,
    string Summary,
    string DetailedReason
);
// أضف هذا الكلاس
public record PredictionResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("predictions")] PredictionData Predictions
);

public record PredictionData(
    [property: JsonPropertyName("median_eta_seconds")] double P50Seconds,
    [property: JsonPropertyName("upper_bound_eta_seconds")] double P90Seconds
);