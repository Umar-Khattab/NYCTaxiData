using MediatR;
using System;
using System.Collections.Generic;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast6h;

/// <summary>
/// Query to predict 6-hour demand for a list of zones.
/// Accepts minimal input parameters from frontend.
/// </summary>
public record GetDemandForecast6hQuery : IRequest<Result<List<Demand6hResult>>>
{
    public List<int> ZoneIds { get; init; } = [];
    public DateTime TargetTime { get; init; }
}
