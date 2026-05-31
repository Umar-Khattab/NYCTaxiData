// NYCTaxiData.API/Controllers/Base/BaseController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Contracts;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Domain.Common;

namespace NYCTaxiData.API.Controllers.Base;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    private ISender? _mediator;

    // ? Lazy Injection „‰ «·‹ Services
    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices
                                 .GetRequiredService<ISender>();

    // =========================================
    #region Success Responses
    // =========================================

    protected IActionResult OkResult<T>(T data, string? message = null)
        => Ok(ApiResponse<T>.Success(data, message));

    protected IActionResult CreatedResult<T>(T data, string? message = null)
        => StatusCode(201, ApiResponse<T>.Success(data, message));

    protected IActionResult PaginatedResult<T>(
        PaginatedList<T> list,
        string? message = null)
        => Ok(ApiResponse<PaginatedList<T>>.Success(list, message));

    #endregion

    // =========================================
    #region Error Responses
    // =========================================

    protected IActionResult NotFoundResult(string message)
        => NotFound(ApiResponse<object>.Fail(message, "NOT_FOUND"));

    protected IActionResult BadRequestResult(string message)
        => BadRequest(ApiResponse<object>.Fail(message, "BAD_REQUEST"));

    protected IActionResult UnauthorizedResult(string message = "Unauthorized")
        => Unauthorized(ApiResponse<object>.Fail(message, "UNAUTHORIZED"));

    protected IActionResult ConflictResult(string message)
        => Conflict(ApiResponse<object>.Fail(message, "CONFLICT"));

    protected IActionResult ServerErrorResult(string message)
        => StatusCode(500, ApiResponse<object>.Fail(message, "SERVER_ERROR"));

    #endregion

    // =========================================
    #region Result Pattern Handlers
    // =========================================

    // ? ··‹ Result<T> «··Ì ›ÌÂ Data
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result is null)
            return NotFoundResult("Resource not found");

        if (result.IsSuccess)
            return OkResult(result.Data!, result.Message);

        return MapError(result.ErrorCode, result.Message);
    }

    // ? ··‹ Result »œÊ‰ Data
    protected IActionResult HandleResult(Result result)
    {
        if (result is null)
            return NotFoundResult("Resource not found");

        if (result.IsSuccess)
            return Ok(ApiResponse<object>.Success(null!, result.Message));

        return MapError(result.ErrorCode, result.Message);
    }

    // ? ··‹ PaginatedList
    protected IActionResult HandlePagedResult<T>(Result<PaginatedList<T>> result)
    {
        if (result is null)
            return NotFoundResult("Resource not found");

        if (result.IsSuccess && result.Data is not null)
            return PaginatedResult(result.Data, result.Message);

        return MapError(result.ErrorCode, result.Message);
    }

    // ? Map Error Codes ·‹ HTTP Status Codes
    private IActionResult MapError(string? errorCode, string? message)
    {
        return errorCode switch
        {
            var c when c?.Contains("NotFound") == true
                => NotFoundResult(message ?? "Resource not found"),

            var c when c?.Contains("Validation") == true
                => BadRequestResult(message ?? "Validation error"),

            var c when c?.Contains("Unauthorized") == true
                => UnauthorizedResult(message ?? "Unauthorized access"),

            var c when c?.Contains("Conflict") == true
                => ConflictResult(message ?? "Conflict error"),

            var c when c?.Contains("ServerError") == true
                => ServerErrorResult(message ?? "Internal server error"),

            _ => BadRequestResult(message ?? "An unexpected error occurred")
        };
    }
    protected IActionResult HandleUnitResult(Result<Unit> result)
    {
        if (result is null) return NotFoundResult("Resource not found");

        if (result.IsSuccess)
            // »‰»⁄  null ·√‰ «·‹ Unit „⁄‰«Â« „›Ì‘ œ« « ›⁄·Ì…
            return Ok(ApiResponse<object>.Success(null!, result.Message));

        return MapError(result.ErrorCode, result.Message);
    }
    #endregion
}