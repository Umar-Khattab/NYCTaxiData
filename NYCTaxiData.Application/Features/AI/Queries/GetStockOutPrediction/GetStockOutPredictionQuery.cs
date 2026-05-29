using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetStockOutPrediction;

/// <summary>
/// Query to predict stock-out probability for a list of zones.
/// </summary>
public record GetStockOutPredictionQuery(
    List<StockOutInput> Zones
) : IRequest<Result<List<StockOutResult>>>;
