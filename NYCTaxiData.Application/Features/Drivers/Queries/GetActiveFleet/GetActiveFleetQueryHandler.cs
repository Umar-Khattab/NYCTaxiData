using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetActiveFleet;

public sealed class GetActiveFleetQueryHandler
    : IRequestHandler<GetActiveFleetQuery, Result<PaginatedList<ActiveFleetDriverDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetActiveFleetQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<ActiveFleetDriverDto>>> Handle(GetActiveFleetQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Drivers.GetPagedAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            predicate: d => d.Status != CurrentStatus.Offline,
            orderBy: query => query.OrderBy(d => d.Fullname));

        var mappedItems = _mapper.Map<IReadOnlyList<ActiveFleetDriverDto>>(items.ToList());

        var paginated = new PaginatedList<ActiveFleetDriverDto>(
            mappedItems,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<ActiveFleetDriverDto>>.Success(paginated);
    }
}
