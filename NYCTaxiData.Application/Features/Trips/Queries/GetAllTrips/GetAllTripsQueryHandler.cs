using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetAllTrips
{
    public class GetAllTripsQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<GetAllTripsQuery, Result<PaginatedList<TripDto>>>
    {
        public async Task<Result<PaginatedList<TripDto>>> Handle(
            GetAllTripsQuery request,
            CancellationToken cancellationToken)
        {
            // Query only non-deleted trips
            var query = _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.DeletedAt == null);

            if (request.StartDate.HasValue)
                query = query.Where(t => t.StartedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(t => t.StartedAt <= request.EndDate.Value);

            if (request.DriverId.HasValue)
                query = query.Where(t => t.DriverId == request.DriverId.Value);

            if (!string.IsNullOrEmpty(request.ProcessStatus))
                query = query.Where(t => t.ProcessStatus == request.ProcessStatus);

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

            return Result<PaginatedList<TripDto>>.Success(paginatedData, "Trips retrieved successfully");
        }
    }
}
