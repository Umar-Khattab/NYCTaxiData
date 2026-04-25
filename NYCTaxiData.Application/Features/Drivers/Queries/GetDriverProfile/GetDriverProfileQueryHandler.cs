using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverProfile;

public sealed class GetDriverProfileQueryHandler : IRequestHandler<GetDriverProfileQuery, Result<DriverProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDriverProfileQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DriverProfileDto>> Handle(GetDriverProfileQuery request, CancellationToken cancellationToken)
    {
        var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);
        if (driver is null)
        {
            return Result<DriverProfileDto>.Failure($"Driver with id '{request.DriverId}' was not found.");
        }

        var allTrips = await _unitOfWork.Trips.FindByConditionAsync(t => t.DriverId == request.DriverId);
        var trips = allTrips.ToList();

        var completedTrips = trips.Count(t => t.EndedAt.HasValue);
        var activeTrips = trips.Count(t => t.StartedAt.HasValue && !t.EndedAt.HasValue);
        var totalEarnings = trips.Where(t => t.ActualFare.HasValue).Sum(t => t.ActualFare ?? 0m);
        var lastTripEndedAt = trips.Where(t => t.EndedAt.HasValue).OrderByDescending(t => t.EndedAt).Select(t => t.EndedAt).FirstOrDefault();

        var profile = _mapper.Map<DriverProfileDto>(driver);
        profile = new DriverProfileDto
        {
            DriverId = profile.DriverId,
            FullName = profile.FullName,
            PlateNumber = profile.PlateNumber,
            LicenseNumber = profile.LicenseNumber,
            Rating = profile.Rating,
            Status = profile.Status,
            PhoneNumber = driver.IdNavigation?.Phonenumber ?? string.Empty,
            Email = driver.IdNavigation?.Email ?? string.Empty,
            CompletedTrips = completedTrips,
            ActiveTrips = activeTrips,
            TotalEarnings = totalEarnings,
            LastTripEndedAt = lastTripEndedAt
        };

        return Result<DriverProfileDto>.Success(profile);
    }
}
