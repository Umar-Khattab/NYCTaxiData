using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.DTOs.Identity; // 🚀 للتوافق مع الـ DriverListDto
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverList;

public sealed class GetDriverListQueryHandler
    : IRequestHandler<GetDriverListQuery, Result<PaginatedList<DriverListDto>>> // 👈 حدثنا الـ Interface هنا
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDriverListQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<DriverListDto>>> Handle(GetDriverListQuery request, CancellationToken cancellationToken) // 👈 حدثنا الـ Return هنا
    {
        CurrentStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<CurrentStatus>(request.Status, true, out var status))
            {
                return Result<PaginatedList<DriverListDto>>.Failure("Invalid status filter.");
            }
            parsedStatus = status;
        }

        Expression<Func<Driver, bool>> predicate = driver =>
            (!parsedStatus.HasValue || driver.Status == parsedStatus.Value.ToString())
            && (!request.ZoneId.HasValue
                || driver.Trips.Any(t =>
                    (t.PickupLocation != null && t.PickupLocation.ZoneId == request.ZoneId.Value)
                    || (t.DropoffLocation != null && t.DropoffLocation.ZoneId == request.ZoneId.Value)));

        // في الـ Handler:
        var (items, totalCount) = await _unitOfWork.Drivers.GetPagedAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            predicate: predicate,
            orderBy: query => query.OrderBy(d => d.FullName),
            d => d.User); // 👈 الـ Include اللي هيحل مشكلة الـ Unknown

        // 🚀 التعديل الذهبي بتاعك شغال الحين 100% وهيقرأ الـ Profile المتقفل صخر
        var mappedItems = _mapper.Map<IReadOnlyList<DriverListDto>>(items.ToList());

        var result = PaginatedList<DriverListDto>.Create(
            mappedItems,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<DriverListDto>>.Success(result, "Driver list retrieved successfully");
    }
}