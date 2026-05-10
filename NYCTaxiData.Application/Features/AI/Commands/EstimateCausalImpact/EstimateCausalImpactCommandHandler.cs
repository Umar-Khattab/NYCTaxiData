using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Features.AI.DTOs;
using NYCTaxiData.Application.Common;

namespace NYCTaxiData.Application.Features.AI.Commands.EstimateCausalImpact;

/// <summary>
/// Handler for <see cref="EstimateCausalImpactCommand"/>.
/// </summary>
public class EstimateCausalImpactCommandHandler : IRequestHandler<EstimateCausalImpactCommand, Result<CausalImpactResult>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly ILogger<EstimateCausalImpactCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EstimateCausalImpactCommandHandler"/> class.
    /// </summary>
    public EstimateCausalImpactCommandHandler(IAiPredictionService aiPredictionService, ILogger<EstimateCausalImpactCommandHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<CausalImpactResult>> Handle(EstimateCausalImpactCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiPredictionService.EstimateCausalImpactAsync(
                request.ZoneId,
                request.EventDate,
                request.TreatmentType,
                request.BaselineDemand,
                request.BaselineDate,
                cancellationToken);

            return Result<CausalImpactResult>.Success(result, "Causal impact estimation generated successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for causal impact estimation");
            throw new ConflictException("ML prediction service is currently unavailable. Please try again later.");
        }
    }
}
