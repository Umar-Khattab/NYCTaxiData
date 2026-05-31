using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetStockOutPrediction;

/// <summary>
/// Query to predict stock-out probability for a list of zones.
/// A zone is considered stocked-out when taxi supply is critically low relative to demand.
/// </summary>
public record GetStockOutPredictionQuery : IRequest<Result<List<StockOutResult>>>
{
    /// <summary>List of NYC taxi zone IDs (1–265) to assess stock-out risk for.</summary>
    public List<int> ZoneIds { get; init; } = [];

    /// <summary>The target date/time for the prediction in ISO 8601 format (e.g. 2024-01-02T06:00:00).</summary>
    public DateTime TargetTime { get; init; }
}
