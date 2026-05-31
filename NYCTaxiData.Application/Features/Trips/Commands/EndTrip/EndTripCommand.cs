using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.DTOs.Trip; // œÂ «·„Â„ ⁄‘«‰ «·‹ TripEndResultDto
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