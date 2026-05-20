using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Features.AI.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictStockOut;

/// <summary>
/// Command to predict stock-out probability for a list of zones.
/// </summary>
public record PredictStockOutCommand(
    List<StockOutInput> Zones
) : IRequest<Result<List<StockOutResult>>>;
