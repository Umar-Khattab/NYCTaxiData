using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.Trips.Commands.DeleteTrip;

public sealed class DeleteTripCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<DeleteTripCommand, Result<TripDeleteResultDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<TripDeleteResultDto>> Handle(DeleteTripCommand request, CancellationToken cancellationToken)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId);
        if (trip == null)
            return Result<TripDeleteResultDto>.Failure($"Trip with ID {request.TripId} not found", "NotFound");

        await _unitOfWork.Trips.DeleteAsync(trip);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<TripDeleteResultDto>(trip);
        return Result<TripDeleteResultDto>.Success(dto, "Trip deleted successfully");
    }
}
