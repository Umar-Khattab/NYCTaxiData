using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripsByZone
{
    public class GetTripsByZoneQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<GetTripsByZoneQuery, Result<PaginatedList<TripDto>>>
    {
        public async Task<Result<PaginatedList<TripDto>>> Handle(
            GetTripsByZoneQuery request,
            CancellationToken cancellationToken)
        {
            var zoneExists = await _unitOfWork.Zones.ExistsAsync(request.ZoneId);
            if (!zoneExists)
                return Result<PaginatedList<TripDto>>.Failure($"Zone with ID {request.ZoneId} not found", "NotFound");

            var query = _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t =>  
                           ((t.PickupLocation != null && t.PickupLocation.ZoneId == request.ZoneId) ||
                            (t.DropoffLocation != null && t.DropoffLocation.ZoneId == request.ZoneId)));

            var totalCount = await query.CountAsync(cancellationToken);

            var dbTrips = await query
                .Include(t => t.PickupLocation).ThenInclude(l => l.Zone)
                .Include(t => t.DropoffLocation).ThenInclude(l => l.Zone)
                .OrderByDescending(t => t.StartedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<TripDto>>(dbTrips);

            var paginatedData = PaginatedList<TripDto>.Create(
                dtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PaginatedList<TripDto>>.Success(paginatedData, "Zone-specific trips retrieved successfully");
        }
    }
}
