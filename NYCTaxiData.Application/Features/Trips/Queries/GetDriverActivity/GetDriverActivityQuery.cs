using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using System.Collections.Generic;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetDriverActivity
{
    public record GetDriverActivityQuery() : IRequest<Result<List<DriverActivityDto>>>;
}
