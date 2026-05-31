using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.DriverAnalytics;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverEarnings
{
    public record GetDriverEarningsQuery(Guid DriverId, string Period)
     : IRequest<Result<DriverEarningsDto>>;
}
