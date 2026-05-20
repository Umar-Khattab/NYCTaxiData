using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictDemand6h;

/// <summary>
/// Command to predict 6-hour demand for a list of zones.
/// </summary>
public record PredictDemand6hCommand(
    List<Demand6hInput> Zones
) : IRequest<Result<List<Demand6hResult>>>;
