using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Zone;
using System.Collections.Generic;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetHeatmapData
{
    public record GetHeatmapDataQuery() : IRequest<Result<List<HeatmapDataPointDto>>>;
}
