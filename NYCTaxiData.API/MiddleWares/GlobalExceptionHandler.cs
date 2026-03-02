using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Contracts;
using System.Net;

namespace NYCTaxiData.API.MiddleWares
{
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
            // 1. تسجيل الخطأ في الـ Logs (مهم جداً للمطورين)
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            // 2. تحديد نوع الخطأ وكود الـ HTTP
            var statusCode = exception switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401
                KeyNotFoundException => (int)HttpStatusCode.NotFound, // 404
                ArgumentException => (int)HttpStatusCode.BadRequest, // 400
                _ => (int)HttpStatusCode.InternalServerError // 500 (الخطأ الافتراضي لأي كارثة)
            };

            // 3. تجهيز الرد (Response) بالشكل الموحد الذي اتفقنا عليه
            var response = new APIResponse<object>
            {
                IsSuccess = false,
                Message = "An error occurred while processing your request.",
                Error = new
                {
                    code = GetErrorCode(statusCode),
                    details = exception.Message // في بيئة الإنتاج (Production) لا يجب إرجاع الـ exception.Message للمستخدم، نكتفي برسالة عامة
                }
            };

            // 4. إرسال الرد للعميل (Frontend/Mobile)
            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true; // تعني أننا تعاملنا مع الخطأ ولن يستمر التطبيق في الانهيار
        }

        private string GetErrorCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "BAD_REQUEST",
                401 => "UNAUTHORIZED",
                403 => "FORBIDDEN",
                404 => "NOT_FOUND",
                503 => "SERVICE_UNAVAILABLE",
                _ => "INTERNAL_SERVER_ERROR"
            };
        }
    }
}