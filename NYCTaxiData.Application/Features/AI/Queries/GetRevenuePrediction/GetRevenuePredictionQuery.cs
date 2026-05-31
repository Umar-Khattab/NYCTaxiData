using MediatR;
using System;
using System.Collections.Generic;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetRevenuePrediction;

/// <summary>
/// Query to predict revenue for a list of zones.
/// Accepts minimal input parameters from frontend.
/// </summary>
public record GetRevenuePredictionQuery : IRequest<Result<List<RevenueResult>>>
{
    public List<int> ZoneIds { get; init; } = [];
    public DateTime TargetTime { get; init; }
}
