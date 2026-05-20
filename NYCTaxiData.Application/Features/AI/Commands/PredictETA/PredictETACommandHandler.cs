using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Features.AI.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictETA;

/// <summary>
/// Handler for <see cref="PredictETACommand"/>.
/// </summary>
public class PredictETACommandHandler : IRequestHandler<PredictETACommand, Result<List<ETAResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly ILogger<PredictETACommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PredictETACommandHandler"/> class.
    /// </summary>
    public PredictETACommandHandler(IAiPredictionService aiPredictionService, ILogger<PredictETACommandHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<ETAResult>>> Handle(PredictETACommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiPredictionService.PredictETAAsync(request.Routes, cancellationToken);
            return Result<List<ETAResult>>.Success(result, "ETA predictions generated successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for ETA prediction");
            throw new ConflictException("ML prediction service is currently unavailable. Please try again later.");
        }
    }
}
