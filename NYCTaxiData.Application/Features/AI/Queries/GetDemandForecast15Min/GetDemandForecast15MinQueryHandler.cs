using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Events;

namespace NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast15Min;

/// <summary>
/// Handler for <see cref="GetDemandForecast15MinQuery"/>.
/// </summary>
public class GetDemandForecast15MinQueryHandler : IRequestHandler<GetDemandForecast15MinQuery, Result<List<Demand15MinResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly IAiFeatureProvider _aiFeatureProvider;
    private readonly IMediator _mediator;
    private readonly ILogger<GetDemandForecast15MinQueryHandler> _logger;

    public GetDemandForecast15MinQueryHandler(IAiPredictionService aiPredictionService, IAiFeatureProvider aiFeatureProvider
        , IMediator mediator, ILogger<GetDemandForecast15MinQueryHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _aiFeatureProvider = aiFeatureProvider;
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<Demand15MinResult>>> Handle(GetDemandForecast15MinQuery request, CancellationToken cancellationToken)
    {
        try
        { 
            var features = await _aiFeatureProvider.GetDemand15MinFeaturesAsync(request.ZoneIds, request.TargetTime, cancellationToken);
             
            var result = await _aiPredictionService.PredictDemand15MinAsync(features, request.RoundToInt, cancellationToken);
             
            await _mediator.Publish(new PredictionGeneratedEvent("Demand15Min", Demand15MinResults: result), cancellationToken);

            return Result<List<Demand15MinResult>>.Success(result, "Demand forecast (15min) generated successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for demand-15min prediction");
            throw new ConflictException("ML prediction service is currently unavailable. Please try again later.");
        }
    }
}
