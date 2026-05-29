using MediatR;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetAllTrips;

public sealed record GetAllTripsQuery(int PageNumber = 1, int PageSize = 50)
    : IRequest<Result<PaginatedList<TripDto>>>, ICacheableQuery;
