using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetShiftStatistics;

public sealed record GetShiftStatisticsQuery(
    Guid DriverId,
    DateTime? ShiftStartUtc = null,
    DateTime? ShiftEndUtc = null)
    : IRequest<Result<ShiftStatisticsDto>>, ICacheableQuery;
