using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Features.AI.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictRevenue;

/// <summary>
/// Handler for <see cref="PredictRevenueCommand"/>.
/// </summary>
public class PredictRevenueCommandHandler : IRequestHandler<PredictRevenueCommand, Result<List<RevenueResult>>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly ILogger<PredictRevenueCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PredictRevenueCommandHandler"/> class.
    /// </summary>
    public PredictRevenueCommandHandler(IAiPredictionService aiPredictionService, ILogger<PredictRevenueCommandHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<List<RevenueResult>>> Handle(PredictRevenueCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiPredictionService.PredictRevenueAsync(request.Zones, cancellationToken);
            return Result<List<RevenueResult>>.Success(result, "Revenue predictions generated successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for revenue prediction");
            throw new ConflictException("ML prediction service is currently unavailable. Please try again later.");
        }
    }
}
