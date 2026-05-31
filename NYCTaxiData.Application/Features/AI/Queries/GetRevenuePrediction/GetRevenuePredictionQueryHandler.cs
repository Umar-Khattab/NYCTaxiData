using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.Common.Exceptions;

namespace NYCTaxiData.Application.Features.AI.Queries.GetRevenuePrediction;

/// <summary>
/// Handler for <see cref="GetRevenuePredictionQuery"/>.
/// </summary>
public class GetRevenuePredictionQueryHandler : IRequestHandler<GetRevenuePredictionQuery, Result<List<RevenueResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly IAiFeatureProvider _aiFeatureProvider;
    private readonly ILogger<GetRevenuePredictionQueryHandler> _logger;

    public GetRevenuePredictionQueryHandler(IAiPredictionService aiPredictionService, 
        IAiFeatureProvider _aiFeatureProvider, ILogger<GetRevenuePredictionQueryHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        this._aiFeatureProvider = _aiFeatureProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<RevenueResult>>> Handle(GetRevenuePredictionQuery request, CancellationToken cancellationToken)
    {
        var features = await _aiFeatureProvider.GetRevenueFeaturesAsync(request.ZoneIds, request.TargetTime, cancellationToken);
        var result = await _aiPredictionService.PredictRevenueAsync(features, cancellationToken);
        
        var predictionDict = result.ToDictionary(r => r.ZoneId);
        var mergedResults = new List<RevenueResult>();
        foreach (var zoneId in request.ZoneIds)
        {
            if (predictionDict.TryGetValue(zoneId, out var pred))
            {
                mergedResults.Add(pred);
            }
            else
            {
                mergedResults.Add(new RevenueResult(zoneId, 0.0, 0.0));
            }
        }

        var sortedResults = mergedResults.OrderBy(r => r.ZoneId).ToList();
        return Result<List<RevenueResult>>.Success(sortedResults, "Revenue prediction generated successfully");
    }
}
