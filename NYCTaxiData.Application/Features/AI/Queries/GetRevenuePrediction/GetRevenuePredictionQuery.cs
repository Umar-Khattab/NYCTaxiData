using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetRevenuePrediction;

/// <summary>
/// Query to predict revenue for a list of zones.
/// </summary>
public record GetRevenuePredictionQuery(
    List<RevenueInput> Zones
) : IRequest<Result<List<RevenueResult>>>;
