using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetEtaPrediction;

/// <summary>
/// Query to predict ETA for a list of zone pairs (routes).
/// </summary>
public record GetEtaPredictionQuery(
    List<ETAInput> Routes
) : IRequest<Result<List<ETAResult>>>;
