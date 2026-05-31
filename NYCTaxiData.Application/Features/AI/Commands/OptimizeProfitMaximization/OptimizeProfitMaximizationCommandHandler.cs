using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Commands.OptimizeProfitMaximization;

/// <summary>
/// Handler for <see cref="OptimizeProfitMaximizationCommand"/>.
/// Calls the AI prediction service to optimize fleet distribution for profit maximization.
/// </summary>
public class OptimizeProfitMaximizationCommandHandler : IRequestHandler<OptimizeProfitMaximizationCommand, Result<ProfitMaximizationResult>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly ILogger<OptimizeProfitMaximizationCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptimizeProfitMaximizationCommandHandler"/> class.
    /// </summary>
    public OptimizeProfitMaximizationCommandHandler(IAiPredictionService aiPredictionService, ILogger<OptimizeProfitMaximizationCommandHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<ProfitMaximizationResult>> Handle(OptimizeProfitMaximizationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiPredictionService.MaximizeProfitAsync(request.ZoneStates, cancellationToken);
            return Result<ProfitMaximizationResult>.Success(result, "Profit maximization plan optimized successfully");
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for profit maximization");
            throw new ConflictException("ML optimization service is currently unavailable. Please try again later.");
        }
    }
}
