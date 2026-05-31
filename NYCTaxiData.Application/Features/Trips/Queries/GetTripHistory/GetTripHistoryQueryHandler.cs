using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Domain.Specifications.Trips; 

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory;

public class GetTripHistoryQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
    : IRequestHandler<GetTripHistoryQuery, Result<PaginatedList<TripHistoryItemDto>>>
{
    public async Task<Result<PaginatedList<TripHistoryItemDto>>> Handle(
        GetTripHistoryQuery request,
        CancellationToken cancellationToken)
    { 
        if (request.DriverId.HasValue)
        {
            var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId.Value);
            if (driver == null)
            {
                return Result<PaginatedList<TripHistoryItemDto>>.Failure($"Driver with ID {request.DriverId} not found");
            }
        }
         
        var spec = new TripHistorySpec(request.DriverId, request.PageNumber, request.PageSize);
         
        var totalCount = await _unitOfWork.Trips.CountAsync(spec);
        var trips = await _unitOfWork.Trips.GetAllBySpecAsync(spec);
         
        var tripItems = _mapper.Map<List<TripHistoryItemDto>>(trips);
         
        var paginatedData = PaginatedList<TripHistoryItemDto>.Create(
            tripItems,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<TripHistoryItemDto>>.Success(paginatedData);
    }
}