using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.API.Controllers.Base;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Application.Features.Trips.Commands.EndTrip;
using NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch;
using NYCTaxiData.Application.Features.Trips.Commands.StartTrip;
using NYCTaxiData.Application.Features.Trips.Queries.GetLiveDispatchFeed;
using NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure;
using NYCTaxiData.Infrastructure.Data.Contexts;
using NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsTrip;

namespace NYCTaxiData.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TripsController
    (IUnitOfWork _unitOfWork,
     IMapper _mapper,
     TaxiDbContext _context,
     ICurrentUserService _currentUserService) : BaseController
{
    // 1. Start Trip
    [HttpPost("start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartTrip([FromBody] StartTripCommand command)
    {
        var data = await Mediator.Send(command);
        return HandleResult(Result.Success(data));
    }

    // 2. End Trip
    [HttpPost("end")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EndTrip([FromBody] EndTripCommand command)
    {
        var data = await Mediator.Send(command);
        return HandleResult(Result.Success(data));
    }

    // 3. Get Trip History (Pagination)
    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] Guid? driverId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var data = await Mediator.Send(new GetTripHistoryQuery(driverId, pageNumber, pageSize));
        return HandleResult(Result.Success(data));
    }

    // 4. Get Online Drivers (Specification Pattern)
    [HttpGet("online")]
    public async Task<IActionResult> GetOnlineDrivers([FromQuery] int page = 1, [FromQuery] int limit = 100)
    {
        var spec = new OnlineDriversSpec(page, limit);
        var drivers = await _unitOfWork.Drivers.GetAllBySpecAsync(spec);
        var totalCount = await _unitOfWork.Drivers.CountAsync(spec);
        var driverDtos = _mapper.Map<List<DriverListDto>>(drivers);

        var data = PaginatedList<DriverListDto>.Create(driverDtos, totalCount, page, limit);
        return HandleResult(Result.Success(data));
    }

    /// <summary>
    /// Retrieves the live dispatch feed.
    /// </summary>
    [HttpGet("live-dispatch")]
    public async Task<IActionResult> GetLiveDispatchFeed([FromQuery] GetLiveDispatchFeedQuery query)
    {
        var data = await Mediator.Send(new GetLiveDispatchFeedQuery(limit, minutesWindow));
        return HandleResult(Result.Success(data));
    }

    /// <summary>
    /// Creates a manual dispatch.
    /// </summary>
    [HttpPost("dispatch")]
    public async Task<IActionResult> ManualDispatch([FromBody] ManualDispatchCommand command)
    {
        var data = await Mediator.Send(command);
        return HandleResult(Result.Success(data));
    }

    // 7. Test Audit (Direct Context for testing Interceptors)
    [HttpPost("test-audit")]
    public async Task<IActionResult> TestAudit()
    {
        var newTrip = new Trip { StartedAt = DateTime.UtcNow };
        _context.Trips.Add(newTrip);
        await _context.SaveChangesAsync();

        var responseData = new
        {
            TripId = newTrip.TripId,
            CreatedBy = newTrip.CreatedBy,
            CreatedAt = newTrip.CreatedAt,
            UserFromToken = _currentUserService.UserName ?? "System"
        };

        return HandleResult(Result.Success(responseData));
    }

    // 8. Delete Trip (Soft Delete Test)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrip(int id)
    { 
        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == id);

        if (trip == null)
            return HandleResult(Result.Failure($"Trip {id} not found", "NotFound"));

        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync();

        return HandleResult(Result.Success(new { trip.TripId, trip.DeletedBy, trip.DeletedAt }));
    }
}