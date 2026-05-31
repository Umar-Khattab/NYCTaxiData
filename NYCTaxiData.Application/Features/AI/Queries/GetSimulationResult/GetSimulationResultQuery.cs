using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetSimulationResult;

/// <summary>
/// Query to retrieve the result of a fleet expansion simulation by ID.
/// Results are cached for 5 minutes to reduce load on the ML service.
/// </summary>
public record GetSimulationResultQuery(
    string SimulationId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<Common.PaginatedList<SimulationResult>>>, ICacheableQuery
{
    /// <inheritdoc />
    public string CacheKey => $"simulation:{SimulationId}";

    /// <inheritdoc />
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
}
