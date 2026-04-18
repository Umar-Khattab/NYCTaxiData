using MediatR;
using AutoMapper;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory
{
    public class GetTripHistoryQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<GetTripHistoryQuery, Result<TripHistoryResultDto>>
    {
        public async Task<Result<TripHistoryResultDto>> Handle(
            GetTripHistoryQuery request,
            CancellationToken cancellationToken)
        {
            // Verify driver exists
            var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);

            if (driver == null)
                throw new NotFoundException($"Driver with ID {request.DriverId} not found");

            // Get paginated trips for the driver
            var (trips, totalCount) = await _unitOfWork.Trips.GetPagedAsync(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                predicate: t => t.DriverId == request.DriverId,
                orderBy: q => q.OrderByDescending(t => t.StartedAt));

            if (!trips.Any())
            {
                return Result<TripHistoryResultDto>.Success(new TripHistoryResultDto
                {
                    CurrentPage = request.PageNumber,
                    TotalPages = 0,
                    TotalCount = 0,
                    Items = []
                });
            }

            // Map trips to DTOs using AutoMapper
            var tripItems = _mapper.Map<List<TripHistoryItemDto>>(trips);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return Result<TripHistoryResultDto>.Success(new TripHistoryResultDto
            {
                CurrentPage = request.PageNumber,
                TotalPages = totalPages,
                TotalCount = totalCount,
                Items = tripItems
            });
        }
    }
}