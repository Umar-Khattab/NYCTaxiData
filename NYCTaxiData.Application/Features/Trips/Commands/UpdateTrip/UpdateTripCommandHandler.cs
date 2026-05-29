using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.Trips.Commands.UpdateTrip;

public sealed class UpdateTripCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdateTripCommand, Result<TripDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<TripDto>> Handle(UpdateTripCommand request, CancellationToken cancellationToken)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId);
        if (trip == null)
            return Result<TripDto>.Failure($"Trip with ID {request.TripId} not found", "NotFound");

        if (request.DriverId.HasValue)
        {
            var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId.Value);
            if (driver == null)
                return Result<TripDto>.Failure($"Driver with ID {request.DriverId} not found", "NotFound");
        }

        if (request.PickupLocationId.HasValue)
        {
            var pickup = await _unitOfWork.Locations.GetByIdAsync(request.PickupLocationId.Value);
            if (pickup == null)
                return Result<TripDto>.Failure($"Pickup location {request.PickupLocationId} not found", "NotFound");
        }

        if (request.DropoffLocationId.HasValue)
        {
            var dropoff = await _unitOfWork.Locations.GetByIdAsync(request.DropoffLocationId.Value);
            if (dropoff == null)
                return Result<TripDto>.Failure($"Dropoff location {request.DropoffLocationId} not found", "NotFound");
        }

        _mapper.Map(request, trip);

        if (request.TotalAmount is null && (request.FareAmount.HasValue || request.TipAmount.HasValue))
        {
            trip.TotalAmount = trip.FareAmount + (trip.TipAmount ?? 0m);
        }

        await _unitOfWork.Trips.UpdateAsync(trip);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<TripDto>(trip);
        return Result<TripDto>.Success(dto, "Trip updated successfully");
    }
}
