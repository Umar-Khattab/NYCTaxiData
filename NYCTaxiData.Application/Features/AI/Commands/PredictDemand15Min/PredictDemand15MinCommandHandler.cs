using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Features.AI.DTOs;
using NYCTaxiData.Application.Common;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictDemand15Min;

/// <summary>
/// Handler for <see cref="PredictDemand15MinCommand"/>.
/// </summary>
public class PredictDemand15MinCommandHandler : IRequestHandler<PredictDemand15MinCommand, Result<List<Demand15MinResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly ILogger<PredictDemand15MinCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PredictDemand15MinCommandHandler"/> class.
    /// </summary>
    public PredictDemand15MinCommandHandler(IAiPredictionService aiPredictionService, ILogger<PredictDemand15MinCommandHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<Demand15MinResult>>> Handle(PredictDemand15MinCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiPredictionService.PredictDemand15MinAsync(request.Zones, request.RoundToInt, cancellationToken);
            return Result<List<Demand15MinResult>>.Success(result, "Demand forecast (15min) generated successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for demand-15min prediction");
            throw new ConflictException("ML prediction service is currently unavailable. Please try again later.");
        }
    }
}
