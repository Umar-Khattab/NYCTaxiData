using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.OptimizeRepositioning;

/// <summary>
/// Handler for <see cref="OptimizeRepositioningCommand"/>.
/// Orchestrates demand prediction, ETA cost matrix building, and optimization via the ML service.
/// </summary>
public class OptimizeRepositioningCommandHandler : IRequestHandler<OptimizeRepositioningCommand, Result<RepositioningPlan>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly ILogger<OptimizeRepositioningCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptimizeRepositioningCommandHandler"/> class.
    /// </summary>
    public OptimizeRepositioningCommandHandler(IAiPredictionService aiPredictionService, ILogger<OptimizeRepositioningCommandHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
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

            return Result<RepositioningPlan>.Success(result, "Repositioning plan optimized successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for repositioning optimization");
            throw new ConflictException("ML optimization service is currently unavailable. Please try again later.");
        }
    }
}
