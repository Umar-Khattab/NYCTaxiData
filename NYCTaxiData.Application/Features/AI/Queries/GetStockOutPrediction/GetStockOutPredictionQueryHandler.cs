using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.Common.Exceptions;

namespace NYCTaxiData.Application.Features.AI.Queries.GetStockOutPrediction;

/// <summary>
/// Handler for <see cref="GetStockOutPredictionQuery"/>.
/// </summary>
public class GetStockOutPredictionQueryHandler : IRequestHandler<GetStockOutPredictionQuery, Result<List<StockOutResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly IAiFeatureProvider _aiFeatureProvider;
    private readonly ILogger<GetStockOutPredictionQueryHandler> _logger;

    public GetStockOutPredictionQueryHandler(IAiPredictionService aiPredictionService, IAiFeatureProvider aiFeatureProvider, ILogger<GetStockOutPredictionQueryHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _aiFeatureProvider = aiFeatureProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<StockOutResult>>> Handle(GetStockOutPredictionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var features = await _aiFeatureProvider.GetStockOutFeaturesAsync(request.ZoneIds, request.TargetTime, cancellationToken);
            var result = await _aiPredictionService.PredictStockOutAsync(features, cancellationToken);
            return Result<List<StockOutResult>>.Success(result, "Stock-out prediction generated successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for stock-out prediction");
            throw new ConflictException("ML prediction service is currently unavailable. Please try again later.");
        }
    }
}
