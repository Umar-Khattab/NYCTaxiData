using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Services.Specifications.Trips; 

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory;

public class GetTripHistoryQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
    : IRequestHandler<GetTripHistoryQuery, TripHistoryResultDto>
{
    public async Task<TripHistoryResultDto> Handle(
      GetTripHistoryQuery request,
      CancellationToken cancellationToken)
    {
        // 1. فحص السائق (فقط لو الـ DriverId مبعوث)
        if (request.DriverId.HasValue)
        {
            var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId.Value); // .Value حلت المشكلة هنا
            if (driver == null)
                throw new NotFoundException($"Driver with ID {request.DriverId} not found");
        }

        // 2. حساب العدد الإجمالي (باستخدام الـ Spec المرنة اللي عملناها)
        // ملاحظة: تأكد إن TripHistorySpec دلوقتي بتقبل Guid? زي ما عدلناها سوا
        var spec = new TripHistorySpec(request.DriverId, request.PageNumber, request.PageSize);

        // يفضل تستخدم CountAsync(spec) مباشرة لو متاحة في الـ Repository
        var totalCount = await _unitOfWork.Trips.CountAsync(spec);

        if (totalCount == 0)
        {
            return new TripHistoryResultDto();
        }

        // 3. جلب البيانات
        var trips = await _unitOfWork.Trips.GetAllBySpecAsync(spec);

        var tripItems = _mapper.Map<List<TripHistoryItemDto>>(trips);

        // 4. التغليف في الـ PaginatedList
        var paginatedData = PaginatedList<TripHistoryItemDto>.Create(
            tripItems,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return TripHistoryResultDto.FromPaginatedList(paginatedData);
    }
}