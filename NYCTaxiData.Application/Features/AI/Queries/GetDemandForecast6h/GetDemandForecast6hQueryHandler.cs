using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.Common.Exceptions;

namespace NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast6h;

/// <summary>
/// Handler for <see cref="GetDemandForecast6hQuery"/>.
/// </summary>
public class GetDemandForecast6hQueryHandler : IRequestHandler<GetDemandForecast6hQuery, Result<List<Demand6hResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly ILogger<GetDemandForecast6hQueryHandler> _logger;

    public GetDemandForecast6hQueryHandler(IAiPredictionService aiPredictionService, ILogger<GetDemandForecast6hQueryHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<Demand6hResult>>> Handle(GetDemandForecast6hQuery request, CancellationToken cancellationToken)
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
