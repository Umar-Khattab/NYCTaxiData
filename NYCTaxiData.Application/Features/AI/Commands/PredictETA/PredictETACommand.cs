using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictETA;

/// <summary>
/// Command to predict ETA for a list of zone pairs (routes).
/// </summary>
public record PredictETACommand(
    List<ETAInput> Routes
) : IRequest<Result<List<ETAResult>>>;
