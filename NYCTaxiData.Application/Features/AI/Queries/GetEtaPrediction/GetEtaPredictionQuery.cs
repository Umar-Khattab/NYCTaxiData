using MediatR;
using System.Collections.Generic;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetEtaPrediction;

/// <summary>
/// Query to predict ETA for a list of zone pairs (routes).
/// Accepts minimal input parameters from frontend.
/// </summary>
public record GetEtaPredictionQuery : IRequest<Result<List<ETAResult>>>
{
    public List<RouteRequest> Routes { get; init; } = [];
}
