using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Events;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast15Min;

/// <summary>
/// Handler for <see cref="GetDemandForecast15MinQuery"/>.
/// </summary>
public class GetDemandForecast15MinQueryHandler : IRequestHandler<GetDemandForecast15MinQuery, Result<List<Demand15MinResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly IAiFeatureProvider _aiFeatureProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<GetDemandForecast15MinQueryHandler> _logger;

    public GetDemandForecast15MinQueryHandler(
        IAiPredictionService aiPredictionService, 
        IAiFeatureProvider aiFeatureProvider,
        IUnitOfWork unitOfWork,
        IMediator mediator, 
        ILogger<GetDemandForecast15MinQueryHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _aiFeatureProvider = aiFeatureProvider;
        _unitOfWork = unitOfWork;
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
             
            var predictionDict = result.ToDictionary(r => r.ZoneId);
            
            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
            var zoneDict = zones.ToDictionary(z => z.ZoneId, z => z);

            var mergedResults = new List<Demand15MinResult>();
            foreach (var zoneId in request.ZoneIds)
            {
                long? osmId = null;
                double? centerLat = null;
                double? centerLong = null;

                if (zoneDict.TryGetValue(zoneId, out var dbZone))
                {
                    osmId = dbZone.OsmId;
                    centerLat = dbZone.CenterLat;
                    centerLong = dbZone.CenterLong;
                }

                if (predictionDict.TryGetValue(zoneId, out var pred))
                {
                    mergedResults.Add(pred with { OsmId = osmId, CenterLatitude = centerLat, CenterLongitude = centerLong });
                }
                else
                {
                    mergedResults.Add(new Demand15MinResult(zoneId, 0.0, null, null, osmId, centerLat, centerLong));
                }
            }

            var sortedResults = mergedResults.OrderBy(r => r.ZoneId).ToList();

            await _mediator.Publish(new PredictionGeneratedEvent("Demand15Min", Demand15MinResults: sortedResults), cancellationToken);

            return Result<List<Demand15MinResult>>.Success(sortedResults, "Demand forecast (15min) generated successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for demand-15min prediction");
            throw new ConflictException("ML prediction service is currently unavailable. Please try again later.");
        }
    }
}
