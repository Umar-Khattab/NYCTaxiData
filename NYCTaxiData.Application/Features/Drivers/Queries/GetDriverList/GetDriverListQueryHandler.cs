using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using System.Linq.Expressions;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverList;

public sealed class GetDriverListQueryHandler
    : IRequestHandler<GetDriverListQuery, Result<PaginatedList<DriverDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDriverListQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<DriverDto>>> Handle(GetDriverListQuery request, CancellationToken cancellationToken)
    {
        CurrentStatus? parsedStatus = null;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<CurrentStatus>(request.Status, true, out var status))
            {
                return Result<PaginatedList<DriverDto>>.Failure("Invalid status filter.");
            }

            parsedStatus = status;
        }

        Expression<Func<NYCTaxiData.Domain.Entities.Driver, bool>> predicate = driver =>
            (!parsedStatus.HasValue || driver.Status == parsedStatus.Value)
            && (!request.ZoneId.HasValue
                || driver.Trips.Any(t =>
                    (t.PickupLocation != null && t.PickupLocation.ZoneId == request.ZoneId.Value)
                    || (t.DropoffLocation != null && t.DropoffLocation.ZoneId == request.ZoneId.Value)));

        var (items, totalCount) = await _unitOfWork.Drivers.GetPagedAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            predicate: predicate,
            orderBy: query => query.OrderBy(d => d.Fullname));

        var mappedItems = _mapper.Map<IReadOnlyList<DriverDto>>(items.ToList());

        var result = new PaginatedList<DriverDto>(
            mappedItems,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<DriverDto>>.Success(result);
    }
}
