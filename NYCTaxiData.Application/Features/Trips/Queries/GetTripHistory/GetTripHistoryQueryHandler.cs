using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Services.Specifications.Trips;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory;

public class GetTripHistoryQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
    : IRequestHandler<GetTripHistoryQuery, Result<PaginatedList<TripHistoryItemDto>>>
{
    public async Task<Result<PaginatedList<TripHistoryItemDto>>> Handle(
        GetTripHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // 1. التحقق من وجود السائق لو الـ DriverId مبعوث
        if (request.DriverId.HasValue)
        {
            var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId.Value);
            if (driver == null)
            {
                return Result<PaginatedList<TripHistoryItemDto>>.Failure($"Driver with ID {request.DriverId} not found");
            }
        }

        // 2. استخدام الـ Specification Pattern اللي إنت لسه عامله
        var spec = new TripHistorySpec(request.DriverId, request.PageNumber, request.PageSize);

        // 3. جلب العدد الكلي والبيانات
        var totalCount = await _unitOfWork.Trips.CountAsync(spec);
        var trips = await _unitOfWork.Trips.GetAllBySpecAsync(spec);

        // 4. عمل Mapping للـ DTOs
        var tripItems = _mapper.Map<List<TripHistoryItemDto>>(trips);

        // 5. التغليف في الـ PaginatedList والرد بـ Result.Success
        var paginatedData = PaginatedList<TripHistoryItemDto>.Create(
            tripItems,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<TripHistoryItemDto>>.Success(paginatedData);
    }
}