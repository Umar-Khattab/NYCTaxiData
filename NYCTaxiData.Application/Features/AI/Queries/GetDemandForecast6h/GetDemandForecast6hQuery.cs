using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast6h;

/// <summary>
/// Query to predict 6-hour demand for a list of zones.
/// </summary>
public record GetDemandForecast6hQuery : IRequest<Result<List<Demand6hResult>>>
{
    /// <summary>List of NYC taxi zone IDs (1–265) to predict demand for.</summary>
    public List<int> ZoneIds { get; init; } = [];

    /// <summary>The target date/time for the prediction in ISO 8601 format (e.g. 2024-01-02T06:00:00).</summary>
    public DateTime TargetTime { get; init; }
}
