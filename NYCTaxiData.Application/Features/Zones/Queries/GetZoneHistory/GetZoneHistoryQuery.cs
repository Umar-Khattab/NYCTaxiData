using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Zone;
using System;
using System.Collections.Generic;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetZoneHistory
{
    public record GetZoneHistoryQuery(int? ZoneId, DateTime StartDate, DateTime EndDate) : IRequest<Result<List<ZoneHistoryDto>>>;
}
