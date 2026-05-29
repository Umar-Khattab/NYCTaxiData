using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.Common.Exceptions;

namespace NYCTaxiData.Application.Features.AI.Queries.GetRevenuePrediction;

/// <summary>
/// Handler for <see cref="GetRevenuePredictionQuery"/>.
/// </summary>
public class GetRevenuePredictionQueryHandler : IRequestHandler<GetRevenuePredictionQuery, Result<List<RevenueResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly ILogger<GetRevenuePredictionQueryHandler> _logger;

    public GetRevenuePredictionQueryHandler(IAiPredictionService aiPredictionService, ILogger<GetRevenuePredictionQueryHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<RevenueResult>>> Handle(GetRevenuePredictionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiPredictionService.PredictRevenueAsync(request.Zones, cancellationToken);
            return Result<List<RevenueResult>>.Success(result, "Revenue prediction generated successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for revenue prediction");
            throw new ConflictException("ML prediction service is currently unavailable. Please try again later.");
        }
    }
}
