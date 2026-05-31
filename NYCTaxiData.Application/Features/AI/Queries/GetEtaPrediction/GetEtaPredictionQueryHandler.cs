using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Plumbing;
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
        catch (Exception ex)
        { 
            if (ex is HttpRequestException httpEx && httpEx.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                _logger.LogError("ML Service returned 422. Details: {Message}", httpEx.Message);
                throw new Exception($"ML Error 422: {httpEx.Message}");
            }
             
            _logger.LogError(ex, "An error occurred during ETA prediction");
            throw;
        }
    }
}
