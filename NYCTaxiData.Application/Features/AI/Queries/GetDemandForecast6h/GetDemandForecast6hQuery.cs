using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast6h;

/// <summary>
/// Query to predict 6-hour demand for a list of zones.
/// </summary>
public record GetDemandForecast6hQuery(
    List<Demand6hInput> Zones
) : IRequest<Result<List<Demand6hResult>>>;
