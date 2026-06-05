using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.AI.Commands.OptimizeProfitMaximization;

/// <summary>
/// Handler for <see cref="OptimizeProfitMaximizationCommand"/>.
/// Calls the AI prediction service to optimize fleet distribution for profit maximization.
/// </summary>
public class OptimizeProfitMaximizationCommandHandler : IRequestHandler<OptimizeProfitMaximizationCommand, Result<ProfitMaximizationResult>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OptimizeProfitMaximizationCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptimizeProfitMaximizationCommandHandler"/> class.
    /// </summary>
    public OptimizeProfitMaximizationCommandHandler(
        IAiPredictionService aiPredictionService, 
        IUnitOfWork unitOfWork,
        ILogger<OptimizeProfitMaximizationCommandHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<ProfitMaximizationResult>> Handle(OptimizeProfitMaximizationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var targetDateTime = request.TargetDateTime;
            if (string.IsNullOrWhiteSpace(targetDateTime))
            {
                targetDateTime = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            }

            var currentZone = request.CurrentZone ?? 1;

            var result = await _aiPredictionService.MaximizeProfitAsync(
                targetDateTime,
                currentZone,
                request.ZoneStates,
                cancellationToken);

            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
            var zoneDict = zones.ToDictionary(z => z.ZoneId, z => z);

            var updatedRepositionPlan = new List<ProfitRepositionPlanItem>();
            foreach (var item in result.RepositionPlan)
            {
                long? fromOsmId = null;
                double? fromLat = null;
                double? fromLong = null;
                if (zoneDict.TryGetValue(item.FromZoneId, out var fromZone))
                {
                    fromOsmId = fromZone.OsmId;
                    fromLat = fromZone.CenterLat;
                    fromLong = fromZone.CenterLong;
                }

                long? toOsmId = null;
                double? toLat = null;
                double? toLong = null;
                if (zoneDict.TryGetValue(item.ToZoneId, out var toZone))
                {
                    toOsmId = toZone.OsmId;
                    toLat = toZone.CenterLat;
                    toLong = toZone.CenterLong;
                }

                updatedRepositionPlan.Add(item with
                {
                    FromOsmId = fromOsmId,
                    FromCenterLatitude = fromLat,
                    FromCenterLongitude = fromLong,
                    ToOsmId = toOsmId,
                    ToCenterLatitude = toLat,
                    ToCenterLongitude = toLong
                });
            }

            var updatedRejectedMoves = new List<ProfitRejectedMoveItem>();
            foreach (var item in result.RejectedMoves)
            {
                long? fromOsmId = null;
                double? fromLat = null;
                double? fromLong = null;
                if (zoneDict.TryGetValue(item.FromZoneId, out var fromZone))
                {
                    fromOsmId = fromZone.OsmId;
                    fromLat = fromZone.CenterLat;
                    fromLong = fromZone.CenterLong;
                }

                long? toOsmId = null;
                double? toLat = null;
                double? toLong = null;
                if (zoneDict.TryGetValue(item.ToZoneId, out var toZone))
                {
                    toOsmId = toZone.OsmId;
                    toLat = toZone.CenterLat;
                    toLong = toZone.CenterLong;
                }

                updatedRejectedMoves.Add(item with
                {
                    FromOsmId = fromOsmId,
                    FromCenterLatitude = fromLat,
                    FromCenterLongitude = fromLong,
                    ToOsmId = toOsmId,
                    ToCenterLatitude = toLat,
                    ToCenterLongitude = toLong
                });
            }

            var updatedZoneEvaluations = new List<ProfitZoneEvaluation>();
            foreach (var item in result.ZoneEvaluations)
            {
                long? osmId = null;
                double? lat = null;
                double? lng = null;
                if (zoneDict.TryGetValue(item.ZoneId, out var zone))
                {
                    osmId = zone.OsmId;
                    lat = zone.CenterLat;
                    lng = zone.CenterLong;
                }

                updatedZoneEvaluations.Add(item with
                {
                    OsmId = osmId,
                    CenterLatitude = lat,
                    CenterLongitude = lng
                });
            }

            var updatedResult = result with
            {
                RepositionPlan = updatedRepositionPlan,
                RejectedMoves = updatedRejectedMoves,
                ZoneEvaluations = updatedZoneEvaluations
            };

            return Result<ProfitMaximizationResult>.Success(updatedResult, "Profit maximization plan optimized successfully");
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for profit maximization");
            throw new ConflictException("ML optimization service is currently unavailable. Please try again later.");
        }
    }
}
