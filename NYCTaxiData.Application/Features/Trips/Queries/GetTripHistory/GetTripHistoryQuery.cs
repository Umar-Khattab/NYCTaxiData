using MediatR;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.DTOs.Trip;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory
{
    public record GetTripHistoryQuery(
        Guid? DriverId,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<TripHistoryResultDto>, ISecureRequest
    {
    } 

     
}