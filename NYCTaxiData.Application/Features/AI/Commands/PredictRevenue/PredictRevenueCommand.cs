using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Features.AI.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictRevenue;

/// <summary>
/// Command to predict revenue for a list of zones.
/// </summary>
public record PredictRevenueCommand(
    List<RevenueInput> Zones
) : IRequest<Result<BatchPredictionResponse<RevenueResult>>>;
