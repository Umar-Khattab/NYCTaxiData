using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.API.Controllers.Base;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Application.Features.Drivers.Commands.UpdateDriverStatus;
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
public class TripsController(
    IUnitOfWork _unitOfWork,
    IMapper _mapper,
    TaxiDbContext _context,
    ICurrentUserService _currentUserService) : BaseController
{ 
    [HttpPost("start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartTrip([FromBody] StartTripCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }
     
    [HttpPost("end")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EndTrip([FromBody] EndTripCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }
     
    [HttpGet("history")]
    public async Task<IActionResult> GetTripHistory([FromQuery] GetTripHistoryQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }
     
    [HttpGet("online")]
    public async Task<IActionResult> GetOnlineDrivers([FromQuery] int page = 1, [FromQuery] int limit = 100)
    {
        var spec = new OnlineDriversSpec(page, limit);
        var drivers = await _unitOfWork.Drivers.GetAllBySpecAsync(spec);
        var totalCount = await _unitOfWork.Drivers.CountAsync(spec);
        var driverDtos = _mapper.Map<List<DriverListDto>>(drivers);

        var pagedData = PaginatedList<DriverListDto>.Create(driverDtos, totalCount, page, limit);
        return PaginatedResult(pagedData, "Online drivers retrieved successfully");
    }
     
    [HttpGet("dispatch/feed")]
    public async Task<IActionResult> GetLiveDispatchFeed([FromQuery] GetLiveDispatchFeedQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }
     
    [HttpPost("dispatch/manual")]
    public async Task<IActionResult> ManualDispatch([FromBody] ManualDispatchCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }
     
    [HttpPatch("driver/status")]
    public async Task<IActionResult> UpdateDriverStatus([FromBody] UpdateDriverStatusCommand command)
    {
        var result = await Mediator.Send(command); 
        return result.IsSuccess
        ? HandleResult(Result<object>.Success(null))
        : HandleResult(Result<object>.Failure(result.Error, "UpdateFailed"));
    }
     
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

        return HandleResult(Result<object>.Success(responseData));
    }
     
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrip(int id)
    {
        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == id);

        if (trip == null)
            return HandleResult(Result<object>.Failure($"Trip {id} not found", "NotFound"));

        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync();

        return HandleResult(Result<object>.Success(new { trip.TripId, trip.DeletedBy, trip.DeletedAt }));
    }
}