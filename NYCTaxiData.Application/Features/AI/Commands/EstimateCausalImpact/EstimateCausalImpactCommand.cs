using System.ComponentModel.DataAnnotations;
using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.EstimateCausalImpact;

/// <summary>
/// Command to estimate the causal impact of a treatment event on demand in a zone.
/// </summary>
public record EstimateCausalImpactCommand(
    [Range(1, 265)] int ZoneId,
    DateTime EventDate,
    string TreatmentType,
    double BaselineDemand,
    DateTime? BaselineDate = null
) : IRequest<Result<CausalImpactResult>>;
