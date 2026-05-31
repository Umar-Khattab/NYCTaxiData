using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.DriverAnalytics;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverAnalytics
{
    public record GetDriverAnalyticsQuery(Guid DriverId, DateTime StartRange, DateTime EndRange)
    : IRequest<Result<DriverAnalyticsDto>>;
}
