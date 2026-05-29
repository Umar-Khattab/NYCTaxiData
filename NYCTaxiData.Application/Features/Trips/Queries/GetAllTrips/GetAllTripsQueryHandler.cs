using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetAllTrips;

public sealed class GetAllTripsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetAllTripsQuery, Result<PaginatedList<TripDto>>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<TripDto>>> Handle(GetAllTripsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Trips.Query()
            .Where(t => t.DeletedAt == null);

        var totalCount = await query.CountAsync(cancellationToken);

        var trips = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var mapped = _mapper.Map<List<TripDto>>(trips);
        var paged = PaginatedList<TripDto>.Create(mapped, totalCount, request.PageNumber, request.PageSize);

        return Result<PaginatedList<TripDto>>.Success(paged, "Trips retrieved successfully");
    }
}
