using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Plumping; // توحيد الـ Result
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverProfile
{
    // تأكد أن الـ GetDriverProfileQuery والـ DriverProfileDto من نفس الـ Namespace ده
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

            // الحسابات
            var completedTrips = trips.Count(t => t.EndedAt.HasValue);
            var activeTrips = trips.Count(t => !t.EndedAt.HasValue);
            var totalEarnings = trips.Sum(t => t.TotalAmount ?? 0m);
            var lastTripEndedAt = trips.Where(t => t.EndedAt.HasValue)
                                       .OrderByDescending(t => t.EndedAt)
                                       .Select(t => t.EndedAt)
                                       .FirstOrDefault();

            // كريت الـ DTO مرة واحدة عشان الـ init
            var profile = new DriverProfileDto
            {
                DriverId = driver.UserId, // استخدام UserId بناءً على الـ Scaffold الجديد
                FullName = driver.FullName ?? string.Empty,
                PlateNumber = driver.PlateNumber,
                LicenseNumber = driver.LicenseNumber,
                Rating = driver.Rating,
                Status = driver.Status.ToString(),
                PhoneNumber = driver.User?.PhoneNumber ?? string.Empty, 
                CompletedTrips = completedTrips,
                ActiveTrips = activeTrips,
                TotalEarnings = totalEarnings,
                LastTripEndedAt = lastTripEndedAt
            };

            return Result<DriverProfileDto>.Success(profile);
        }
    }
}