using MediatR;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.DTOs.Trip;
using System;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetAllTrips
{
    public record GetAllTripsQuery(
        int PageNumber = 1,
        int PageSize = 10,
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        Guid? DriverId = null,
        string? ProcessStatus = null
    ) : IRequest<Result<PaginatedList<TripDto>>>;
}
