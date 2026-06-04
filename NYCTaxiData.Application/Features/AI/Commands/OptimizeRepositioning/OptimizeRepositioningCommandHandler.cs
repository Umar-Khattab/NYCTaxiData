using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.AI.Commands.OptimizeRepositioning;

/// <summary>
/// Handler for <see cref="OptimizeRepositioningCommand"/>.
/// Orchestrates demand prediction, ETA cost matrix building, and optimization via the ML service.
/// </summary>
public class OptimizeRepositioningCommandHandler : IRequestHandler<OptimizeRepositioningCommand, Result<RepositioningPlan>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OptimizeRepositioningCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptimizeRepositioningCommandHandler"/> class.
    /// </summary>
    public OptimizeRepositioningCommandHandler(
        IAiPredictionService aiPredictionService, 
        IUnitOfWork unitOfWork,
        ILogger<OptimizeRepositioningCommandHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<RepositioningPlan>> Handle(OptimizeRepositioningCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiPredictionService.OptimizeRepositioningAsync(
                request.TimeWindow,
                request.ZoneStates,
                request.Constraints,
                cancellationToken);

            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
            var zoneDict = zones.ToDictionary(z => z.ZoneId, z => z);

            var updatedAssignments = new List<RepositionAssignment>();
            foreach (var assignment in result.Assignments)
            {
                long? fromOsmId = null;
                double? fromLat = null;
                double? fromLong = null;
                if (zoneDict.TryGetValue(assignment.FromZoneId, out var fromZone))
                {
                    fromOsmId = fromZone.OsmId;
                    fromLat = fromZone.CenterLat;
                    fromLong = fromZone.CenterLong;
                }

                long? toOsmId = null;
                double? toLat = null;
                double? toLong = null;
                if (zoneDict.TryGetValue(assignment.ToZoneId, out var toZone))
                {
                    toOsmId = toZone.OsmId;
                    toLat = toZone.CenterLat;
                    toLong = toZone.CenterLong;
                }

                updatedAssignments.Add(assignment with
                {
                    FromOsmId = fromOsmId,
                    FromCenterLatitude = fromLat,
                    FromCenterLongitude = fromLong,
                    ToOsmId = toOsmId,
                    ToCenterLatitude = toLat,
                    ToCenterLongitude = toLong
                });
            }

            var updatedSummaries = new List<ZonePlanSummary>();
            foreach (var summary in result.ZoneSummaries)
            {
                long? osmId = null;
                double? lat = null;
                double? lng = null;
                if (zoneDict.TryGetValue(summary.ZoneId, out var zone))
                {
                    osmId = zone.OsmId;
                    lat = zone.CenterLat;
                    lng = zone.CenterLong;
                }

                updatedSummaries.Add(summary with
                {
                    OsmId = osmId,
                    CenterLatitude = lat,
                    CenterLongitude = lng
                });
            }

            var updatedPlan = result with
            {
                Assignments = updatedAssignments,
                ZoneSummaries = updatedSummaries
            };

            return Result<RepositioningPlan>.Success(updatedPlan, "Repositioning plan optimized successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for repositioning optimization");
            throw new ConflictException("ML optimization service is currently unavailable. Please try again later.");
        }
    }
}
