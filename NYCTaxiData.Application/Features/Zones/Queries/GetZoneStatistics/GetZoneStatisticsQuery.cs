using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Zone;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetZoneStatistics
{
    public record GetZoneStatisticsQuery(int? ZoneId) : IRequest<Result<ZoneStatisticsDto>>;
}
