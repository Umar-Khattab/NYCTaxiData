using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripsByZone;

public sealed class GetTripsByZoneQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetTripsByZoneQuery, Result<PaginatedList<TripDto>>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<TripDto>>> Handle(GetTripsByZoneQuery request, CancellationToken cancellationToken)
    {
        var zone = await _unitOfWork.Zones.GetByIdAsync(request.ZoneId);
        if (zone == null)
            return Result<PaginatedList<TripDto>>.Failure($"Zone with ID {request.ZoneId} not found", "NotFound");

        var zoneLocationIds = _unitOfWork.Locations.Query()
            .Where(l => l.ZoneId == request.ZoneId)
            .Select(l => l.LocationId);

        var query = _unitOfWork.Trips.Query()
            .Where(t => t.DeletedAt == null &&
                        ((t.PickupLocationId.HasValue && zoneLocationIds.Contains(t.PickupLocationId.Value))
                        || (t.DropoffLocationId.HasValue && zoneLocationIds.Contains(t.DropoffLocationId.Value))));

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
