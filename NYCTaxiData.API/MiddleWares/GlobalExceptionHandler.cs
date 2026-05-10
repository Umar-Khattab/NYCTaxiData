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
        _logger.LogError(exception,
            "Exception occurred: {Message} | Path: {Path} | Time: {Time}",
            exception.Message,
            httpContext.Request.Path,
            DateTime.UtcNow);
         
        var (statusCode, errorCode) = exception switch
        {
            NotFoundException => (404, "NOT_FOUND"),
            ValidationException => (400, "VALIDATION_ERROR"),  
            UnauthorizedException => (401, "UNAUTHORIZED"),
            ConflictException => (409, "CONFLICT"),
            UnauthorizedAccessException => (401, "UNAUTHORIZED"),
            KeyNotFoundException => (404, "NOT_FOUND"),
            ArgumentException => (400, "BAD_REQUEST"),
            _ => (500, "INTERNAL_SERVER_ERROR")
        };
         
        object? validationErrors = null;
        if (exception is ValidationException ve)
        { 
            validationErrors = ve.Errors.SelectMany(e => e.Value).ToList();
        }
         
        var response = ApiResponse<object>.Fail(
            message: statusCode == 500 ? "Internal Server Error" : exception.Message,
            errorCode: errorCode,
            errors: (List<string>?)validationErrors);
         
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}