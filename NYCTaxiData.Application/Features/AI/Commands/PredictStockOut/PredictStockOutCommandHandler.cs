using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Features.AI.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictStockOut;

/// <summary>
/// Handler for <see cref="PredictStockOutCommand"/>.
/// </summary>
public class PredictStockOutCommandHandler : IRequestHandler<PredictStockOutCommand, Result<List<StockOutResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly ILogger<PredictStockOutCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PredictStockOutCommandHandler"/> class.
    /// </summary>
    public PredictStockOutCommandHandler(IAiPredictionService aiPredictionService, ILogger<PredictStockOutCommandHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<StockOutResult>>> Handle(PredictStockOutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiPredictionService.PredictStockOutAsync(request.Zones, cancellationToken);
            return Result<List<StockOutResult>>.Success(result, "Stock-out predictions generated successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for stock-out prediction");
            throw new ConflictException("ML prediction service is currently unavailable. Please try again later.");
        }
    }
}
