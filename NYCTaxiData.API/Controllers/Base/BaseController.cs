using MediatR;
using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Contracts;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Plumping;

namespace NYCTaxiData.API.Controllers.Base;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    private ISender? _mediator;

    // حقن الـ Mediator تلقائياً من الـ Services
    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    #region Success Responses

    // رد نجاح عادي (Data + Message)
    protected IActionResult OkResult<T>(T data, string? message = null)
        => Ok(ApiResponse<T>.Success(data, message));

    // رد إنشاء مورد جديد (Created 201)
    protected IActionResult CreatedResult<T>(T data, string? message = null)
        => StatusCode(201, ApiResponse<T>.Success(data, message));

    // رد خاص بالبيانات المقسمة صفحات (Pagination)
    protected IActionResult PaginatedResult<T>(PaginatedList<T> list, string? message = null)
        => Ok(ApiResponse<PaginatedList<T>>.Success(list, message));

    #endregion

    #region Error Responses

    protected IActionResult NotFoundResult(string message)
        => NotFound(ApiResponse<object>.Fail(message, "NOT_FOUND"));

    protected IActionResult BadRequestResult(string message)
        => BadRequest(ApiResponse<object>.Fail(message, "BAD_REQUEST"));

    protected IActionResult UnauthorizedResult(string message = "Unauthorized")
        => Unauthorized(ApiResponse<object>.Fail(message, "UNAUTHORIZED"));

    protected IActionResult ConflictResult(string message)
        => Conflict(ApiResponse<object>.Fail(message, "CONFLICT"));

    #endregion

    #region Result Pattern Handlers

    // ✅ النوع الأول: للهندلة لما يكون فيه Data راجعة (Result<T>)
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result == null) return NotFound();

        if (result.IsSuccess)
        {
            // لو البيانات عبارة عن قائمة مقسمة، نستخدم PaginatedResult
            if (result.Data is PaginatedList<object>) // ملاحظة: قد تحتاج تعديل النوع حسب الـ Implementation
                return Ok(ApiResponse<T>.Success(result.Data, result.Message));

            return OkResult(result.Data!, result.Message);
        }

        return MapError(result.ErrorCode, result.Message);
    }

    // ✅ النوع الثاني: للهندلة لما يكون الرد نجاح/فشل فقط (Result)
    protected IActionResult HandleResult(Result result)
    {
        if (result == null) return NotFound();

        if (result.IsSuccess)
            return Ok(ApiResponse<object>.Success(null!, result.Message));

        return MapError(result.ErrorCode, result.Message);
    }

    // ميثود مساعدة لتوحيد منطق الأخطاء ومنع تكرار الكود
    private IActionResult MapError(string? errorCode, string? message)
    {
        return errorCode switch
        {
            var c when c?.Contains("NotFound") == true => NotFoundResult(message ?? "المورد غير موجود"),
            var c when c?.Contains("Validation") == true => BadRequestResult(message ?? "خطأ في البيانات المرسلة"),
            var c when c?.Contains("Unauthorized") == true => UnauthorizedResult(message ?? "غير مصرح لك بالقيام بهذا الإجراء"),
            var c when c?.Contains("Conflict") == true => ConflictResult(message ?? "يوجد تعارض في البيانات"),
            _ => BadRequestResult(message ?? "حدث خطأ غير متوقع")
        };
    }

    #endregion
}