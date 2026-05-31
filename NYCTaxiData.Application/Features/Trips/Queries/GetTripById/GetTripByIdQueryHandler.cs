using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripById
{
    public class GetTripByIdQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<GetTripByIdQuery, Result<TripDto>>
    {
        public async Task<Result<TripDto>> Handle(
            GetTripByIdQuery request,
            CancellationToken cancellationToken)
        {
            var trip = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Include(t => t.PickupLocation).ThenInclude(l => l.Zone)
                .Include(t => t.DropoffLocation).ThenInclude(l => l.Zone)
                .FirstOrDefaultAsync(t => t.TripId == request.TripId && t.DeletedAt == null, cancellationToken);

            if (trip == null)
                return Result<TripDto>.Failure($"Trip with ID {request.TripId} not found", "NotFound");

            var dto = _mapper.Map<TripDto>(trip);
            return Result<TripDto>.Success(dto, "Trip retrieved successfully");
        }
    }
}
