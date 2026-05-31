using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Controllers.Base;
using NYCTaxiData.Application.Features.AI.Commands.OptimizeProfitMaximization;
using NYCTaxiData.Application.Features.AI.Commands.OptimizeRepositioning;
using NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast15Min;
using NYCTaxiData.Application.Features.AI.Queries.GetDemandForecast6h;
using NYCTaxiData.Application.Features.AI.Queries.GetEtaPrediction;
using NYCTaxiData.Application.Features.AI.Queries.GetRevenuePrediction;
using NYCTaxiData.Application.Features.AI.Queries.GetStockOutPrediction;

namespace NYCTaxiData.API.Controllers;

/// <summary>
/// API controller for AI/ML prediction and optimization endpoints.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/ai")]
[Asp.Versioning.ApiVersion("1.0")]
public class AiController : BaseController
{
    /// <summary>
    /// Predicts 15-minute demand for a list of zones.
    /// </summary>
    [HttpPost("predict/demand-15min")]
    public async Task<IActionResult> PredictDemand15Min(
        [FromBody] GetDemandForecast15MinQuery query, CancellationToken ct)
        => HandleResult(await Mediator.Send(query, ct));

    /// <summary>
    /// Predicts 6-hour demand for a list of zones.
    /// </summary>
    [HttpPost("predict/demand-6h")]
    public async Task<IActionResult> PredictDemand6h(
        [FromBody] GetDemandForecast6hQuery query, CancellationToken ct)
        => HandleResult(await Mediator.Send(query, ct));

    /// <summary>
    /// Predicts ETA for a list of zone pairs (routes).
    /// </summary>
    [HttpPost("predict/eta")]
    public async Task<IActionResult> PredictETA(
        [FromBody] GetEtaPredictionQuery query, CancellationToken ct)
        => HandleResult(await Mediator.Send(query, ct));

    /// <summary>
    /// Predicts revenue for a list of zones.
    /// </summary>
    [HttpPost("predict/revenue")]
    public async Task<IActionResult> PredictRevenue(
        [FromBody] GetRevenuePredictionQuery query, CancellationToken ct)
        => HandleResult(await Mediator.Send(query, ct));

    /// <summary>
    /// Predicts stock-out probability for a list of zones.
    /// </summary>
    [HttpPost("predict/stockout")]
    public async Task<IActionResult> PredictStockOut(
        [FromBody] GetStockOutPredictionQuery query, CancellationToken ct)
        => HandleResult(await Mediator.Send(query, ct));

    /// <summary>
    /// Optimizes vehicle repositioning across zones.
    /// </summary>
    [HttpPost("optimize/repositioning")]
    public async Task<IActionResult> OptimizeRepositioning(
        [FromBody] OptimizeRepositioningCommand command, CancellationToken ct)
        => HandleResult(await Mediator.Send(command, ct));

    /// <summary>
    /// Optimizes vehicle distribution across zones to maximize profit.
    /// </summary>
    [HttpPost("optimize/profit-maximization")]
    public async Task<IActionResult> OptimizeProfitMaximization(
        [FromBody] OptimizeProfitMaximizationCommand command, CancellationToken ct)
        => HandleResult(await Mediator.Send(command, ct));
}
