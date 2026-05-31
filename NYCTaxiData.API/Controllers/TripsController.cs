using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Controllers.Base;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Application.Features.Drivers.Commands.UpdateDriverStatus;
using NYCTaxiData.Application.Features.Trips.Commands.CreateTrip;
using NYCTaxiData.Application.Features.Trips.Commands.DeleteTrip;
using NYCTaxiData.Application.Features.Trips.Commands.EndTrip;
using NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch;
using NYCTaxiData.Application.Features.Trips.Commands.StartTrip;
using NYCTaxiData.Application.Features.Trips.Commands.UpdateTrip;
using NYCTaxiData.Application.Features.Trips.Queries.GetAllTrips;
using NYCTaxiData.Application.Features.Trips.Queries.GetDemandStatistics;
using NYCTaxiData.Application.Features.Trips.Queries.GetDriverActivity;
using NYCTaxiData.Application.Features.Trips.Queries.GetLiveDispatchFeed;
using NYCTaxiData.Application.Features.Trips.Queries.GetPeakHours;
using NYCTaxiData.Application.Features.Trips.Queries.GetRevenueStatistics;
using NYCTaxiData.Application.Features.Trips.Queries.GetTripById;
using NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory;
using NYCTaxiData.Application.Features.Trips.Queries.GetTripsByZone;
using NYCTaxiData.Application.Features.Trips.Queries.GetTripsStatistics;
using NYCTaxiData.Application.Features.Trips.Queries.GetTripTrends;
using NYCTaxiData.Application.Features.Trips.Queries.GetZoneStatistics;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Domain.Specifications.Drivers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NYCTaxiData.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TripsController(
    IUnitOfWork _unitOfWork,
    IMapper _mapper,
    ICurrentUserService _currentUserService) : BaseController
{
    // ==========================================
    // CORE CRUD ENDPOINTS
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> GetAllTrips(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? driverId = null,
        [FromQuery] string? processStatus = null)
    {
        return HandleResult(await Mediator.Send(new GetAllTripsQuery(pageNumber, pageSize, startDate, endDate, driverId, processStatus)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTripById(int id)
    {
        return HandleResult(await Mediator.Send(new GetTripByIdQuery(id)));
    }

    [HttpGet("zone/{zoneId}")]
    public async Task<IActionResult> GetTripsByZone(int zoneId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        return HandleResult(await Mediator.Send(new GetTripsByZoneQuery(zoneId, pageNumber, pageSize)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTrip([FromBody] CreateTripCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTrip(int id, [FromBody] UpdateTripCommand command)
    {
        if (id != command.TripId)
            return BadRequest("ID in URL must match ID in body.");

        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrip(int id)
    {
        return HandleResult(await Mediator.Send(new DeleteTripCommand(id)));
    }

    // ==========================================
    // PRESERVED LEGACY ACTION ENDPOINTS
    // ==========================================

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
        var spec = new AvailableDriversSpec(page, limit);

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
        : HandleResult(Result<object>.Failure(result.Message ?? "UpdateFailed", "UpdateFailed"));
    }

    /// <remarks>
    /// This endpoint is intended for development/audit verification only.
    /// It is restricted to administrators and development environments.
    /// </remarks>
    [HttpPost("test-audit")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TestAudit([FromServices] NYCTaxiData.Infrastructure.Data.Contexts.TaxiDbContext context)
    {
        if (!HttpContext.RequestServices
                .GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>()
                .IsDevelopment())
        {
            return Forbid();
        }

        var newTrip = new NYCTaxiData.Domain.Entities.Trip { StartedAt = DateTime.UtcNow };
        context.Trips.Add(newTrip);
        await context.SaveChangesAsync();

        var responseData = new
        {
            TripId = newTrip.TripId,
            CreatedBy = newTrip.CreatedBy,
            CreatedAt = newTrip.CreatedAt,
            UserFromToken = _currentUserService.UserName ?? "System"
        };

        return HandleResult(Result<object>.Success(responseData));
    }

    // ==========================================
    // DETAILED TRIP ANALYTICS ENDPOINTS
    // ==========================================

    [HttpGet("statistics")]
    public async Task<IActionResult> GetTripsStatistics()
    {
        return HandleResult(await Mediator.Send(new GetTripsStatisticsQuery()));
    }

    [HttpGet("statistics/revenue")]
    public async Task<IActionResult> GetRevenueStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return HandleResult(await Mediator.Send(new GetRevenueStatisticsQuery(startDate, endDate)));
    }

    [HttpGet("statistics/demand")]
    public async Task<IActionResult> GetDemandStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return HandleResult(await Mediator.Send(new GetDemandStatisticsQuery(startDate, endDate)));
    }

    [HttpGet("statistics/zones")]
    public async Task<IActionResult> GetZoneStatistics()
    {
        return HandleResult(await Mediator.Send(new GetZoneStatisticsQuery()));
    }

    [HttpGet("statistics/peak-hours")]
    public async Task<IActionResult> GetPeakHours()
    {
        return HandleResult(await Mediator.Send(new GetPeakHoursQuery()));
    }

    [HttpGet("statistics/trends")]
    public async Task<IActionResult> GetTripTrends()
    {
        return HandleResult(await Mediator.Send(new GetTripTrendsQuery()));
    }

    [HttpGet("statistics/drivers")]
    public async Task<IActionResult> GetDriverActivity()
    {
        return HandleResult(await Mediator.Send(new GetDriverActivityQuery()));
    }
}