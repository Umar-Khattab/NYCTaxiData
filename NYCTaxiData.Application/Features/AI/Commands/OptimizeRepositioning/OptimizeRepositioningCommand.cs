using MediatR;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Commands.OptimizeRepositioning;

/// <summary>
/// Command to optimize vehicle repositioning across zones based on supply/demand state.
/// This command is transactional to ensure database atomicity when persisting the plan.
/// </summary>
public record OptimizeRepositioningCommand(
    DateTime TimeWindow,
    List<ZoneSupplyState> ZoneStates,
    OptimizationConstraints? Constraints = null
) : IRequest<Result<RepositioningPlan>>, ITransactionalCommand;
