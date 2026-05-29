using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripById;

public sealed class GetTripByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetTripByIdQuery, Result<TripDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<TripDto>> Handle(GetTripByIdQuery request, CancellationToken cancellationToken)
    {
        var trip = await _unitOfWork.Trips.Query()
            .FirstOrDefaultAsync(t => t.TripId == request.TripId && t.DeletedAt == null, cancellationToken);

        if (trip == null)
            return Result<TripDto>.Failure($"Trip with ID {request.TripId} not found", "NotFound");

        var dto = _mapper.Map<TripDto>(trip);
        return Result<TripDto>.Success(dto, "Trip retrieved successfully");
    }
}
