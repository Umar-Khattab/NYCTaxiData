using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.StartFleetExpansionSimulation;

/// <summary>
/// Handler for <see cref="StartFleetExpansionSimulationCommand"/>.
/// Initiates a fleet expansion simulation and returns a job tracking response.
/// </summary>
public class StartFleetExpansionSimulationCommandHandler : IRequestHandler<StartFleetExpansionSimulationCommand, Result<SimulationJobResponse>>
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly ILogger<StartFleetExpansionSimulationCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartFleetExpansionSimulationCommandHandler"/> class.
    /// </summary>
    public StartFleetExpansionSimulationCommandHandler(IAiPredictionService aiPredictionService, ILogger<StartFleetExpansionSimulationCommandHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<SimulationJobResponse>> Handle(StartFleetExpansionSimulationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var simulationId = Guid.NewGuid().ToString("N")[..12];

            var result = await _aiPredictionService.StartSimulationAsync(
                request.BaseScenarioDate,
                request.AdditionalVehicles,
                request.Strategy,
                request.SimulationDurationHours,
                request.OperationalCostPerVehiclePerDay,
                request.TargetZones,
                cancellationToken);

            var response = result with { SimulationId = simulationId };

            return Result<SimulationJobResponse>.Success(response, "Fleet expansion simulation started successfully");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ML service for fleet expansion simulation");
            throw new ConflictException("ML simulation service is currently unavailable. Please try again later.");
        }
    }
}
