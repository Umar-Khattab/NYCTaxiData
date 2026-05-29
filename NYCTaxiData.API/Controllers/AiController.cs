using Microsoft.AspNetCore.Mvc;
using MediatR;
using NYCTaxiData.API.Contracts;
using NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast15Min;
using NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast6h;
using NYCTaxiData.Application.Features.AI.Queries.GetEtaPrediction;
using NYCTaxiData.Application.Features.AI.Queries.GetRevenuePrediction;
using NYCTaxiData.Application.Features.AI.Queries.GetStockOutPrediction;
using NYCTaxiData.Application.Features.AI.Commands.OptimizeRepositioning;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Application.DTOs;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.API.Controllers;

/// <summary>
/// API controller for AI/ML prediction and optimization endpoints.
/// </summary>
[ApiController]
[Route("api/v1/ai")]
public class AiController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiController"/> class.
    /// </summary>
    public AiController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Predicts 15-minute demand for a list of zones.
    /// </summary>
    [HttpPost("predict/demand-15min")]
    public async Task<ActionResult<ApiResponse<List<Demand15MinResult>>>> PredictDemand15Min(
        [FromBody] GetDemandForecast15MinQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Predicts 6-hour demand for a list of zones.
    /// </summary>
    [HttpPost("predict/demand-6h")]
    public async Task<ActionResult<ApiResponse<BatchPredictionResponse<Demand6hResult>>>> PredictDemand6h(
        [FromBody] GetDemandForecast6hQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Predicts ETA for a list of zone pairs (routes).
    /// </summary>
    [HttpPost("predict/eta")]
    public async Task<ActionResult<ApiResponse<BatchPredictionResponse<ETAResult>>>> PredictETA(
        [FromBody] GetEtaPredictionQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Predicts revenue for a list of zones.
    /// </summary>
    [HttpPost("predict/revenue")]
    public async Task<ActionResult<ApiResponse<BatchPredictionResponse<RevenueResult>>>> PredictRevenue(
        [FromBody] GetRevenuePredictionQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Predicts stock-out probability for a list of zones.
    /// </summary>
    [HttpPost("predict/stockout")]
    public async Task<ActionResult<ApiResponse<BatchPredictionResponse<StockOutResult>>>> PredictStockOut(
        [FromBody] GetStockOutPredictionQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }



    // ========================================================================
    // NEW endpoints - Optimization & Simulation
    // ========================================================================

    /// <summary>
    /// Optimizes vehicle repositioning across zones.
    /// </summary>
    [HttpPost("optimize/repositioning")]
    public async Task<ActionResult<ApiResponse<RepositioningPlan>>> OptimizeRepositioning(
        [FromBody] OptimizeRepositioningCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}
