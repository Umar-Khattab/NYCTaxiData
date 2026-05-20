using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictDemand6h;

/// <summary>
/// Handler for <see cref="PredictDemand6hCommand"/>.
/// </summary>
public class PredictDemand6hCommandHandler : IRequestHandler<PredictDemand6hCommand, Result<List<Demand6hResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly ILogger<PredictDemand6hCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PredictDemand6hCommandHandler"/> class.
    /// </summary>
    public PredictDemand6hCommandHandler(IAiPredictionService aiPredictionService, ILogger<PredictDemand6hCommandHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<Demand6hResult>>> Handle(PredictDemand6hCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiPredictionService.PredictDemand6hAsync(request.Zones, cancellationToken);
            return Result<List<Demand6hResult>>.Success(result, "Demand forecast (6h) generated successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for demand-6h prediction");
            throw new ConflictException("ML prediction service is currently unavailable. Please try again later.");
        }
    }
}
