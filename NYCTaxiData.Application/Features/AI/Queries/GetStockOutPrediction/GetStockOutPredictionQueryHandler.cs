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

    public GetStockOutPredictionQueryHandler(IAiPredictionService aiPredictionService,
        IAiFeatureProvider aiFeatureProvider, ILogger<GetStockOutPredictionQueryHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _aiFeatureProvider = aiFeatureProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<StockOutResult>>> Handle(GetStockOutPredictionQuery request, CancellationToken cancellationToken)
    {
        // 1. جلب البيانات
        var features = await _aiFeatureProvider.GetStockOutFeaturesAsync(request.ZoneIds, request.TargetTime, cancellationToken);

        // 2. التحقق من وجود بيانات قبل إرسال الطلب (هذا هو الحل الجذري!)
        if (features == null || !features.Any())
        {
            _logger.LogWarning("(Features) data is not available for forecasting at the required time.");
            return Result<List<StockOutResult>>.Success(new List<StockOutResult>(), "There is currently no data available for forecasting these areas..");
        }

        // 3. الإرسال فقط إذا كانت هناك بيانات
        var result = await _aiPredictionService.PredictStockOutAsync(features, cancellationToken);
        return Result<List<StockOutResult>>.Success(result, "The prediction was successfully generated.");
    }
}
