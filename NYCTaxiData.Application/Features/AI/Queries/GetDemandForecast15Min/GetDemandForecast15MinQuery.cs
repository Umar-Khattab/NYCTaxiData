using MediatR;
using System;
using System.Collections.Generic;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast15Min;

/// <summary>
/// Query to predict 15-minute demand for a list of zones.
/// Accepts minimal input parameters from frontend.
/// </summary>
public record GetDemandForecast15MinQuery(
    List<int> ZoneIds,
    DateTime TargetTime,
    bool RoundToInt = true
) : IRequest<Result<List<Demand15MinResult>>>;
