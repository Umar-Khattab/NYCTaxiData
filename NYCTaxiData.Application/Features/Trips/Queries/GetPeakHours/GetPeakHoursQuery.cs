using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Trip;
using System.Collections.Generic;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetPeakHours
{
    public record GetPeakHoursQuery() : IRequest<Result<List<TripPeakHoursDto>>>;
}
