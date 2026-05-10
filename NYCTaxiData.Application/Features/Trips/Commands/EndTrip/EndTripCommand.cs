using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.DTOs.Trip; // ده المهم عشان الـ TripEndResultDto
using System;

namespace NYCTaxiData.Application.Features.Trips.Commands.EndTrip
{
    public record EndTripCommand(
        int TripId,
        decimal FarePerMinute = 0.5m,
        decimal BaseFare = 2.50m,
        decimal SurgeMultiplier = 1.0m
    ) : IRequest<Result<TripEndResultDto>>, ITransactionalCommand, ISecureRequest;
}