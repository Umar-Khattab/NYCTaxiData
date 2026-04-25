using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetActiveFleet;

public sealed record GetActiveFleetQuery(
    int PageNumber = 1,
    int PageSize = 10)
    : IRequest<Result<PaginatedList<ActiveFleetDriverDto>>>, ICacheableQuery;
