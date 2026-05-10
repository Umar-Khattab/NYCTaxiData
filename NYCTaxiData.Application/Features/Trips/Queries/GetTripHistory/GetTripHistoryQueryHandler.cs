using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Services.Specifications.Trips;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory
{
    public class GetTripHistoryQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<GetTripHistoryQuery, Result<PaginatedList<TripHistoryItemDto>>>
    {
        public async Task<Result<PaginatedList<TripHistoryItemDto>>> Handle(
            GetTripHistoryQuery request,
            CancellationToken cancellationToken)
        { 
            var page = request.PageNumber > 0 ? request.PageNumber : 1;
            var size = request.PageSize > 0 ? request.PageSize : 10;
             
            if (request.DriverId.HasValue)
            {
                var driverExists = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId.Value);
                if (driverExists == null)
                {
                    return Result<PaginatedList<TripHistoryItemDto>>.Failure($"Driver with ID {request.DriverId} not found", "NotFound");
                }
            }
             
            var spec = new TripHistorySpec(request.DriverId, page, size);
             
            var trips = await _unitOfWork.Trips.GetAllBySpecAsync(spec);
            var totalCount = await _unitOfWork.Trips.CountAsync(spec);
             
            var tripItems = _mapper.Map<List<TripHistoryItemDto>>(trips);
             
            var paginatedData = PaginatedList<TripHistoryItemDto>.Create(
                tripItems,
                totalCount,
                page,
                size);

            return Result<PaginatedList<TripHistoryItemDto>>.Success(paginatedData, "Trip history retrieved successfully");
        }
    }
}