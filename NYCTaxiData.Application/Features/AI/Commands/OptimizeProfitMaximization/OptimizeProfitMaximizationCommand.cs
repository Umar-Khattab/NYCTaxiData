using System.Collections.Generic;
using MediatR;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Commands.OptimizeProfitMaximization;

/// <summary>
/// Command to optimize vehicle repositioning across zones to maximize profit in the next 6 hours.
/// </summary>
public record OptimizeProfitMaximizationCommand(
    string TargetDateTime,
    int CurrentZone,
    List<ProfitMaximizationInput> ZoneStates
) : IRequest<Result<ProfitMaximizationResult>>, ITransactionalCommand;
