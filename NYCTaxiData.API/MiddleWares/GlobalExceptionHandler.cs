using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Contracts;
using NYCTaxiData.Application.Common.Exceptions;
using System.Net;

namespace NYCTaxiData.API.MiddleWares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. تسجيل الخطأ مع المسار (Path) عشان نعرف الـ API اللي ضربت فين بالظبط
        _logger.LogError(exception,
            "Exception occurred: {Message} | Path: {Path} | Time: {Time}",
            exception.Message,
            httpContext.Request.Path,
            DateTime.UtcNow);

        // 2. دمج منطق تحديد الـ StatusCode والـ ErrorCode (القديم والجديد)
        var (statusCode, errorCode) = exception switch
        {
            // أنواع الـ Exceptions اللي إنت ضفتها (Custom)
            NotFoundException => (404, "NOT_FOUND"),
            ValidationException => (400, "VALIDATION_ERROR"),
            UnauthorizedException => (401, "UNAUTHORIZED"),
            ConflictException => (409, "CONFLICT"),

            // أنواع الـ Exceptions الـ Standard (اللي كانت عند صاحبك)
            UnauthorizedAccessException => (401, "UNAUTHORIZED"),
            KeyNotFoundException => (404, "NOT_FOUND"),
            ArgumentException => (400, "BAD_REQUEST"),

            // أي حاجة تانية تعتبر كارثة داخلية
            _ => (500, "INTERNAL_SERVER_ERROR")
        };

        // 3. تجهيز الـ Response باستخدام الـ ApiResponse.Fail الموحد
        // هنا بنعمل حتة صياعة: لو فيه Validation Errors بنسحبها ونبعتها
        var response = ApiResponse<object>.Fail(
            message: exception.Message,
            errorCode: errorCode,
            errors: exception is ValidationException ve
                       ? ve.Errors.SelectMany(e => e.Value).ToList()
                       : null);

        // 4. إرسال الرد
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true; // تم التعامل مع الخطأ بنجاح
    }
}