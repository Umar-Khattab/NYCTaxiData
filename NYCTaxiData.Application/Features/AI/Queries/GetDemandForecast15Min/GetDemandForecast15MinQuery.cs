using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast15Min;

/// <summary>
/// Query to predict 15-minute demand for a list of zones.
/// </summary>
public record GetDemandForecast15MinQuery(
    List<Demand15MinInput> Zones,
    bool RoundToInt = true
) : IRequest<Result<List<Demand15MinResult>>>;
