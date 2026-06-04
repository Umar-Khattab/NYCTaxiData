using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.AI.Queries.GetRevenuePrediction;

/// <summary>
/// Handler for <see cref="GetRevenuePredictionQuery"/>.
/// </summary>
public class GetRevenuePredictionQueryHandler : IRequestHandler<GetRevenuePredictionQuery, Result<List<RevenueResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly IAiFeatureProvider _aiFeatureProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetRevenuePredictionQueryHandler> _logger;

    public GetRevenuePredictionQueryHandler(
        IAiPredictionService aiPredictionService, 
        IAiFeatureProvider aiFeatureProvider, 
        IUnitOfWork unitOfWork,
        ILogger<GetRevenuePredictionQueryHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _aiFeatureProvider = aiFeatureProvider;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<RevenueResult>>> Handle(GetRevenuePredictionQuery request, CancellationToken cancellationToken)
    {
        var features = await _aiFeatureProvider.GetRevenueFeaturesAsync(request.ZoneIds, request.TargetTime, cancellationToken);
        var result = await _aiPredictionService.PredictRevenueAsync(features, cancellationToken);
        
        var predictionDict = result.ToDictionary(r => r.ZoneId);

        var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
        var zoneDict = zones.ToDictionary(z => z.ZoneId, z => z);

        var mergedResults = new List<RevenueResult>();
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
                mergedResults.Add(new RevenueResult(zoneId, 0.0, 0.0, osmId, centerLat, centerLong));
            }
        }

        var sortedResults = mergedResults.OrderBy(r => r.ZoneId).ToList();
        return Result<List<RevenueResult>>.Success(sortedResults, "Revenue prediction generated successfully");
    }
}
