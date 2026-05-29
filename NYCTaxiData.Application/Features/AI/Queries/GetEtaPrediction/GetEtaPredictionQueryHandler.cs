using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.Common.Exceptions;

namespace NYCTaxiData.Application.Features.AI.Queries.GetEtaPrediction;

/// <summary>
/// Handler for <see cref="GetEtaPredictionQuery"/>.
/// </summary>
public class GetEtaPredictionQueryHandler : IRequestHandler<GetEtaPredictionQuery, Result<List<ETAResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly IAiFeatureProvider _aiFeatureProvider;
    private readonly ILogger<GetEtaPredictionQueryHandler> _logger;

    public GetEtaPredictionQueryHandler(IAiPredictionService aiPredictionService, IAiFeatureProvider aiFeatureProvider, ILogger<GetEtaPredictionQueryHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _aiFeatureProvider = aiFeatureProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<ETAResult>>> Handle(GetEtaPredictionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var features = await _aiFeatureProvider.GetEtaFeaturesAsync(request.Routes, cancellationToken);
            var result = await _aiPredictionService.PredictETAAsync(features, cancellationToken);
            return Result<List<ETAResult>>.Success(result, "ETA prediction generated successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for ETA prediction");
            throw new ConflictException("ML prediction service is currently unavailable. Please try again later.");
        }
    }
}
