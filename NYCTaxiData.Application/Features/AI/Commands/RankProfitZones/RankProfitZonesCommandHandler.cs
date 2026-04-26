using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Features.AI.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.RankProfitZones;

/// <summary>
/// Handler for <see cref="RankProfitZonesCommand"/>.
/// </summary>
public class RankProfitZonesCommandHandler : IRequestHandler<RankProfitZonesCommand, Result<List<ProfitZoneResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly ILogger<RankProfitZonesCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RankProfitZonesCommandHandler"/> class.
    /// </summary>
    public RankProfitZonesCommandHandler(IAiPredictionService aiPredictionService, ILogger<RankProfitZonesCommandHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<ProfitZoneResult>>> Handle(RankProfitZonesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await _aiPredictionService.RankZonesByProfitAsync(
                request.ZoneIds,
                request.CurrentHour,
                request.DayOfWeek,
                request.ConsiderStockOutRisk,
                request.TopK,
                cancellationToken);

            if (request.TopK.HasValue && request.TopK.Value < results.Count)
            {
                results = results.Take(request.TopK.Value).ToList();
            }

            return Result<List<ProfitZoneResult>>.Success(results, $"Ranked {results.Count} zones by profit successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for profit zone ranking");
            throw new ConflictException("ML prediction service is currently unavailable. Please try again later.");
        }
    }
}
