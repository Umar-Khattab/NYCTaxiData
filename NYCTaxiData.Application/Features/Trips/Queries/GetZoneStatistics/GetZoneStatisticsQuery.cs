using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Zone;
using System.Collections.Generic;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetZoneStatistics
{
    // Represents trip aggregates grouped by zone (pickups and dropoffs)
    public record GetZoneStatisticsQuery() : IRequest<Result<List<ZoneStatisticsDto>>>;
}
