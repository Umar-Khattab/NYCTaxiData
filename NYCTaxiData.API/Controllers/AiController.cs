using Microsoft.AspNetCore.Mvc;
using MediatR;
using NYCTaxiData.API.Contracts;
using NYCTaxiData.Application.Features.AI.Commands.PredictDemand15Min;
using NYCTaxiData.Application.Features.AI.Commands.PredictDemand6h;
using NYCTaxiData.Application.Features.AI.Commands.PredictETA;
using NYCTaxiData.Application.Features.AI.Commands.PredictRevenue;
using NYCTaxiData.Application.Features.AI.Commands.PredictStockOut;
using NYCTaxiData.Application.Features.AI.Commands.OptimizeRepositioning;
using NYCTaxiData.Application.Features.AI.DTOs;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Application.DTOs;
using NYCTaxiData.Domain.DTOs;

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

    // ========================================================================
    // EXISTING endpoints (keep them - do not remove)
    // ========================================================================
    // [HttpGet("demand-forecast")]
    // [HttpGet("dispatch-recommendation")]
    // [HttpGet("optimal-driver-schedule")]
    // [HttpGet("explainable-ai-insight")]
    // [HttpPost("voice-assistant")]
    // [HttpPost("simulate/operational")]
    // [HttpPost("simulate/strategic")]
    // [HttpPost("model/retrain")]

    // ========================================================================
    // NEW endpoints - Demand Predictions
    // ========================================================================

    /// <summary>
    /// Predicts 15-minute demand for a list of zones.
    /// </summary>
    [HttpPost("predict/demand-15min")]
    public async Task<ActionResult<ApiResponse<List<Domain.DTOs.Demand15MinResult>>>> PredictDemand15Min(
        [FromBody] PredictDemand15MinCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Predicts 6-hour demand for a list of zones.
    /// </summary>
    [HttpPost("predict/demand-6h")]
    public async Task<ActionResult<ApiResponse<Domain.DTOs.BatchPredictionResponse<Domain.DTOs.Demand6hResult>>>> PredictDemand6h(
        [FromBody] PredictDemand6hCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Predicts ETA for a list of zone pairs (routes).
    /// </summary>
    [HttpPost("predict/eta")]
    public async Task<ActionResult<ApiResponse<Domain.DTOs.BatchPredictionResponse<Domain.DTOs.ETAResult>>>> PredictETA(
        [FromBody] PredictETACommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Predicts revenue for a list of zones.
    /// </summary>
    [HttpPost("predict/revenue")]
    public async Task<ActionResult<ApiResponse<Domain.DTOs.BatchPredictionResponse<Domain.DTOs.RevenueResult>>>> PredictRevenue(
        [FromBody] PredictRevenueCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Predicts stock-out probability for a list of zones.
    /// </summary>
    [HttpPost("predict/stockout")]
    public async Task<ActionResult<ApiResponse<Domain.DTOs.BatchPredictionResponse<Domain.DTOs.StockOutResult>>>> PredictStockOut(
        [FromBody] PredictStockOutCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }



    // ========================================================================
    // NEW endpoints - Optimization & Simulation
    // ========================================================================

    /// <summary>
    /// Optimizes vehicle repositioning across zones.
    /// </summary>
    [HttpPost("optimize/repositioning")]
    public async Task<ActionResult<ApiResponse<Domain.DTOs.RepositioningPlan>>> OptimizeRepositioning(
        [FromBody] OptimizeRepositioningCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}
