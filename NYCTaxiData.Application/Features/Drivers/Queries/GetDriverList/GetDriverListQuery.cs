using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverList;

public sealed record GetDriverListQuery(
    string? Status,
    int? ZoneId,
    int PageNumber = 1,
    int PageSize = 10)
    : IRequest<Result<PaginatedList<DriverDto>>>, ICacheableQuery;
