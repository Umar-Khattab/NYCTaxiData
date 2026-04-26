using NYCTaxiData.Domain.Enums;

namespace NYCTaxiData.Application.Features.AI.DTOs;

/// <summary>
/// Standard wrapper for batch prediction responses returned by the ML service.
/// </summary>
/// <typeparam name="T">Type of the individual prediction result.</typeparam>
public record BatchPredictionResponse<T>(
    string RequestId,
    DateTime PredictedAt,
    string ModelVersion,
    int Count,
    List<T> Results,
    PredictionMetadata Metadata
);

/// <summary>
/// Metadata about the prediction model and inference performance.
/// </summary>
public record PredictionMetadata(
    string ModelName,
    string ModelVersion,
    long InferenceTimeMs,
    int InputCount
);

/// <summary>
/// A complete repositioning plan with assignments and zone summaries.
/// </summary>
public record RepositioningPlan(
    string PlanId,
    DateTime CreatedAt,
    DateTime ValidUntil,
    int TotalMoves,
    int VehiclesMoved,
    double EstimatedRelocationCost,
    double EstimatedRevenueGain,
    double NetProfitImpact,
    List<RepositionAssignment> Assignments,
    List<ZonePlanSummary> ZoneSummaries
);

/// <summary>
/// Response returned when a fleet expansion simulation job is started.
/// </summary>
public record SimulationJobResponse(
    string SimulationId,
    SimulationStatus Status,
    DateTime CreatedAt,
    string? ResultUrl
);

/// <summary>
/// Full result of a completed fleet expansion simulation.
/// </summary>
public record SimulationResult(
    string SimulationId,
    SimulationStatus Status,
    DateTime CompletedAt,
    SimulationMetrics BaselineMetrics,
    SimulationMetrics SimulatedMetrics,
    FinancialImpact FinancialImpact,
    List<ZoneSimulationResult> ZoneBreakdown,
    SimulationRecommendation Recommendation
);
