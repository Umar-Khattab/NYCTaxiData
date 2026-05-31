using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.Common.Exceptions;

namespace NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast6h;

/// <summary>
/// Handler for <see cref="GetDemandForecast6hQuery"/>.
/// </summary>
public class GetDemandForecast6hQueryHandler : IRequestHandler<GetDemandForecast6hQuery, Result<List<Demand6hResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly IAiFeatureProvider _aiFeatureProvider;
    private readonly ILogger<GetDemandForecast6hQueryHandler> _logger;

    public GetDemandForecast6hQueryHandler(IAiPredictionService aiPredictionService, IAiFeatureProvider aiFeatureProvider, ILogger<GetDemandForecast6hQueryHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _aiFeatureProvider = aiFeatureProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<Demand6hResult>>> Handle(GetDemandForecast6hQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var features = await _aiFeatureProvider.GetDemand6hFeaturesAsync(request.ZoneIds, request.TargetTime, cancellationToken);
            var result = await _aiPredictionService.PredictDemand6hAsync(features, cancellationToken);
            return Result<List<Demand6hResult>>.Success(result, "Demand forecast (6h) generated successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for demand-6h prediction");
            throw new ConflictException("ML prediction service is currently unavailable. Please try again later.");
        }
    }
}
