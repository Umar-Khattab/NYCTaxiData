using MediatR;
using System;
using System.Collections.Generic;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetStockOutPrediction;

/// <summary>
/// Query to predict stock-out probability for a list of zones.
/// Accepts minimal input parameters from frontend.
/// </summary>
public record GetStockOutPredictionQuery(
    List<int> ZoneIds,
    DateTime TargetTime
) : IRequest<Result<List<StockOutResult>>>;
