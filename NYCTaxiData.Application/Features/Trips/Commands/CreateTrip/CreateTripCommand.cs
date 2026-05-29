using MediatR;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;

namespace NYCTaxiData.Application.Features.Trips.Commands.CreateTrip;

public sealed record CreateTripCommand(
    Guid? DriverId,
    int? PickupLocationId,
    int? DropoffLocationId,
    decimal FareAmount,
    decimal? TipAmount,
    decimal? TotalAmount,
    DateTime? StartedAt,
    DateTime? EndedAt,
    string? ProcessStatus,
    string? CvDataPath)
    : IRequest<Result<TripDto>>, ITransactionalCommand, ISecureRequest;
