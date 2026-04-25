using MediatR;
using NYCTaxiData.Application.Common;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverProfile;

public sealed record GetDriverProfileQuery(Guid DriverId) : IRequest<Result<DriverProfileDto>>;
