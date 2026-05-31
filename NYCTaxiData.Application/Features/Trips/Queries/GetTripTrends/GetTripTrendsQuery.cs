using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Trip;
using System.Collections.Generic;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripTrends
{
    public record GetTripTrendsQuery() : IRequest<Result<List<TripTrendDto>>>;
}
