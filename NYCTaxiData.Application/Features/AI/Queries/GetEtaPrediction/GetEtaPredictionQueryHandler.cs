using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.Common.Exceptions;

namespace NYCTaxiData.Application.Features.AI.Queries.GetEtaPrediction;

/// <summary>
/// Handler for <see cref="GetEtaPredictionQuery"/>.
/// </summary>
public class GetEtaPredictionQueryHandler : IRequestHandler<GetEtaPredictionQuery, Result<List<ETAResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly IAiFeatureProvider _aiFeatureProvider;
    private readonly ILogger<GetEtaPredictionQueryHandler> _logger;

    public GetEtaPredictionQueryHandler(IAiPredictionService aiPredictionService, IAiFeatureProvider aiFeatureProvider, ILogger<GetEtaPredictionQueryHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _aiFeatureProvider = aiFeatureProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<ETAResult>>> Handle(GetEtaPredictionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var features = await _aiFeatureProvider.GetEtaFeaturesAsync(request.Routes, cancellationToken);
            var result = await _aiPredictionService.PredictETAAsync(features, cancellationToken);
            
            var predictionMap = result.GroupBy(r => (r.PickupZoneId, r.DropoffZoneId))
                                      .ToDictionary(g => g.Key, g => g.First());
            var mergedResults = new List<ETAResult>();
            foreach (var route in request.Routes)
            {
                var key = (route.PickupZoneId, route.DropoffZoneId);
                if (predictionMap.TryGetValue(key, out var pred))
                {
                    mergedResults.Add(pred);
                }
                else
                {
                    mergedResults.Add(new ETAResult(route.PickupZoneId, route.DropoffZoneId, 180.0, 300.0));
                }
            }

            var sortedResults = mergedResults.OrderBy(r => r.PickupZoneId).ThenBy(r => r.DropoffZoneId).ToList();
            return Result<List<ETAResult>>.Success(sortedResults, "ETA prediction generated successfully");
        }
        catch (Exception ex)
        { 
            if (ex is HttpRequestException httpEx && httpEx.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                _logger.LogError("ML Service returned 422. Details: {Message}", httpEx.Message);
                throw new Exception($"ML Error 422: {httpEx.Message}");
            }
             
            _logger.LogError(ex, "An error occurred during ETA prediction");
            throw;
        }
    }
}
