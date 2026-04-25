using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace NYCTaxiData.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var timer = Stopwatch.StartNew();

        // 1. بداية التنفيذ مع تسجيل الـ Request Object بالكامل (Destructuring)
        // علامة الـ @ بتخلي الـ Logger يسجل الـ Properties بتاعة الأوبجكت كـ JSON
        _logger.LogInformation(
            "[START] Handling Request: {RequestName} | Data: {@Request}",
            requestName, request);

        try
        {
            // 2. الانتقال للخطوة اللي بعدها في الـ Pipeline (مثلاً الـ Validation)
            var response = await next();
            timer.Stop();

            // 3. تسجيل النجاح مع الوقت المستغرق
            _logger.LogInformation(
                "[END] {RequestName} completed successfully | Elapsed: {ElapsedMs}ms",
                requestName, timer.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();

            // 4. تسجيل الكوارث (Errors) مع تفاصيل الاستثناء
            _logger.LogError(ex,
                "[ERROR] {RequestName} failed after {ElapsedMs}ms | Message: {ErrorMessage}",
                requestName, timer.ElapsedMilliseconds, ex.Message);

            throw; // بنعمل re-throw عشان الـ GlobalExceptionHandler يلقطها
        }
    }
}