using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast15Min;

/// <summary>
/// Query to predict 15-minute demand for a list of zones.
/// </summary>
public record GetDemandForecast15MinQuery : IRequest<Result<List<Demand15MinResult>>>
{
    /// <summary>List of NYC taxi zone IDs (1–265) to predict demand for.</summary>
    public List<int> ZoneIds { get; init; } = [];

    /// <summary>The target date/time for the prediction in ISO 8601 format (e.g. 2024-01-02T06:00:00).</summary>
    public DateTime TargetTime { get; init; }

    /// <summary>When true, prediction values are rounded to the nearest integer. Default: true.</summary>
    public bool RoundToInt { get; init; } = true;
}
