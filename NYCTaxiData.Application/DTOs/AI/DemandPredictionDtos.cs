using NYCTaxiData.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NYCTaxiData.Application.DTOs.AI;

/// <summary>
/// Input features for 15-minute demand prediction per zone.
/// </summary>
public record Demand15MinInput(
    [Range(1, 265)] int ZoneId,
    [Range(0, 23)] int Hour,
    [Range(0, 59)] int Minute,
    [Range(0, 6)] int DayOfWeek,
    [Range(1, 12)] int Month,
    bool IsWeekend,
    double Lag1,
    double Lag4,
    double Lag96,
    double RollMean1h,
    double RollMean3h,
    double TempC,
    [Range(0, double.MaxValue)] double RainMm,
    bool IsRain,
    int WeatherCode,
    [Range(0, int.MaxValue)] int PickupCount
);

/// <summary>
/// Result of a 15-minute demand prediction for a single zone.
/// </summary>
public record Demand15MinResult(
    int ZoneId,
    double P50,
    double P90,
    double LowerBound,
    double UpperBound
);

/// <summary>
/// Input features for 6-hour demand prediction per zone.
/// </summary>
public record Demand6hInput(
    [Range(1, 265)] int ZoneId,
    [Range(0, 23)] int PickupHour,
    [Range(0, 6)] int DayOfWeek,
    bool IsWeekend,
    bool IsHoliday,
    double Lag1_6h,
    double Lag2_6h,
    double Lag4_6h,
    double RollingMean24h,
    double TempC,
    [Range(0, double.MaxValue)] double RainMm,
    bool IsRain,
    int WeatherCode,
    [Range(0, int.MaxValue)] int PickupCount
);

/// <summary>
/// Result of a 6-hour demand prediction for a single zone.
/// </summary>
public record Demand6hResult(
    int ZoneId,
    double P50,
    double P90,
    double ConfidenceIntervalLower,
    double ConfidenceIntervalUpper
);

/// <summary>
/// Input features for ETA prediction between two zones.
/// </summary>
public record ETAInput(
    [Range(1, 265)] int PickupZoneId,
    [Range(1, 265)] int DropoffZoneId,
    decimal? TempC,
    decimal? RainMm,
    int? WeatherCode,
    decimal DistanceProxy,
    int PUHour,
    int PUDow,
    int PUMonth,
    int PUMinute,
    bool IsWeekend,
    bool IsRushHour,
    DateTime PU15MinBucket,
    string DistanceBucketLabel,
    decimal DurationSec,
    decimal OdHourMedianDuration,
    decimal PUHourSlowdownIndex,
    int DistMedianDuration
);

/// <summary>
/// Result of an ETA prediction for a zone pair.
/// </summary>
public record ETAResult(
    int PickupZoneId,
    int DropoffZoneId,
    double P50Seconds,
    double P90Seconds
)
{
    /// <summary>
    /// Median ETA in minutes.
    /// </summary>
    public double P50Minutes => P50Seconds / 60;

    /// <summary>
    /// 90th percentile ETA in minutes.
    /// </summary>
    public double P90Minutes => P90Seconds / 60;
}

/// <summary>
/// Input features for revenue prediction per zone.
/// </summary>
public record RevenueInput(
    [Range(1, 265)] int ZoneId,
    [Range(0, 23)] int PickupHour,
    [Range(0, 6)] int DayOfWeek,
    bool IsWeekend,
    int lag1_6h,
    int lag2_6h,
    int lag4_6h,
    double RevLag1_6h,
    double RevLag1Week,
    double RevRollingMean7d,
    double RevRollingMean30d,
    decimal? RollingMean24h,
    [Range(0, double.MaxValue)] double AvgFare,
    [Range(0, 1)] double TipRate,
    double? TempC,
    [Range(0, double.MaxValue)] double? RainMm,
    bool? IsRain,
    int? WeatherCode,
    bool? IsHoliday
);

/// <summary>
/// Result of a revenue prediction for a single zone.
/// </summary>
public record RevenueResult(
    int ZoneId,
    double P50,
    double P90,
    double ExpectedTotalRevenue
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
    int ZoneId,
    double Probability
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
    string AssignmentId,
    int FromZoneId,
    int ToZoneId,
    int VehicleCount,
    double ETAMinutes,
    double EstimatedCost,
    double ExpectedProfit
);

/// <summary>
/// Summary of supply and demand for a zone after optimization.
/// </summary>
public record ZonePlanSummary(
    int ZoneId,
    int SupplyBefore,
    int SupplyAfter,
    double DemandForecast,
    double CoverageRatioAfter
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
