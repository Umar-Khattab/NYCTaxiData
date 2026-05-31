using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetEtaPrediction;

/// <summary>
/// Query to predict ETA for a list of zone pairs (pickup → dropoff routes).
/// </summary>
public record GetEtaPredictionQuery : IRequest<Result<List<ETAResult>>>
{
    /// <summary>List of pickup/dropoff zone pairs with target departure time.</summary>
    public List<RouteRequest> Routes { get; init; } = [];
}
