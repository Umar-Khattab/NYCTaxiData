using MediatR;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.DTOs.Trip;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripsByZone
{
    public record GetTripsByZoneQuery(
        int ZoneId,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Result<PaginatedList<TripDto>>>;
}
