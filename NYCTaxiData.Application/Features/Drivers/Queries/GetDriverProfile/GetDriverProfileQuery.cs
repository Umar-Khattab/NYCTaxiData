using MediatR;
using NYCTaxiData.Application.Common.Plumbing; // استخدام المسار الموحد للـ Result

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverProfile;

public sealed record GetDriverProfileQuery(Guid DriverId) : IRequest<Result<DriverProfileDto>>;