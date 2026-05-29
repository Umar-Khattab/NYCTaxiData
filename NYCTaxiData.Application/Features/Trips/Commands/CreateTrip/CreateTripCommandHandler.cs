using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.Trips.Commands.CreateTrip;

public sealed class CreateTripCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<CreateTripCommand, Result<TripDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<TripDto>> Handle(CreateTripCommand request, CancellationToken cancellationToken)
    {
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

        var trip = _mapper.Map<Trip>(request);
        trip.CreatedAt = DateTime.UtcNow;
        trip.TotalAmount ??= request.FareAmount + (request.TipAmount ?? 0m);

        await _unitOfWork.Trips.AddAsync(trip);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<TripDto>(trip);
        return Result<TripDto>.Success(dto, "Trip created successfully");
    }
}
