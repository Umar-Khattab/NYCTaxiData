using MediatR;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.DTOs.Trip;
using System;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory
{
    // 1.  ⁄—Ì› «·‹ Query «·„ÊÕœ…
    // «” Œœ„‰« PageNumber Ê PageSize ⁄‘«‰   „«‘Ï „⁄ «·‹ PaginatedList «··Ì ›Ì «·‹ Handler
    public record GetTripHistoryQuery(
        Guid? DriverId = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Result<PaginatedList<TripHistoryItemDto>>>, ISecureRequest;
}