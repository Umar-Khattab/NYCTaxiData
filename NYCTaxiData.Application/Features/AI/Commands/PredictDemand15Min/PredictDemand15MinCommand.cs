using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Features.AI.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictDemand15Min;

/// <summary>
/// Command to predict 15-minute demand for a list of zones.
/// </summary>
public record PredictDemand15MinCommand(
    List<Demand15MinInput> Zones,
    bool RoundToInt = true
) : IRequest<Result<List<Demand15MinResult>>>;
