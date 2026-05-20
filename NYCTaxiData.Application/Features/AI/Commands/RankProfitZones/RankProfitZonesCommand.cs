using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.RankProfitZones;

/// <summary>
/// Command to rank zones by expected profit.
/// </summary>
public record RankProfitZonesCommand(
    List<int> ZoneIds,
    int CurrentHour,
    int DayOfWeek,
    bool ConsiderStockOutRisk = true,
    int? TopK = null
) : IRequest<Result<List<ProfitZoneResult>>>;
