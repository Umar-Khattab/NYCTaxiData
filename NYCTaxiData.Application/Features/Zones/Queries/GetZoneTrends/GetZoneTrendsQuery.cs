using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Zone;
using System.Collections.Generic;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetZoneTrends
{
    public record GetZoneTrendsQuery(int? ZoneId, string TrendType = "hourly") : IRequest<Result<List<ZoneTrendDto>>>;
}
