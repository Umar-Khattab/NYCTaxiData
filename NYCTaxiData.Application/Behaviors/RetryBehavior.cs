using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Npgsql;  
using NYCTaxiData.Application.Common.Exceptions;
using Polly;
using Polly.Retry;

namespace NYCTaxiData.Application.Behaviors
{
    internal class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;
        private const int DefaultRetryCount = 3;

        public RetryBehavior(ILogger<RetryBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var retryCount = GetRetryCountForRequest(requestName);

            var retryPolicy = Policy
                .Handle<Exception>(IsTransientError)
                .WaitAndRetryAsync(
                    retryCount: retryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                    onRetry: (outcome, timespan, retryNumber, context) =>
                    {
                        _logger.LogWarning("Retrying {RequestName}... Attempt {RetryNumber}/{RetryCount}. Error: {Message}",
                            requestName, retryNumber, retryCount, outcome.Message);
                    });

            return await retryPolicy.ExecuteAsync(async (ct) => await next(), cancellationToken);
        }

        private bool IsTransientError(Exception exception)
        {
            // 1. أخطاء الـ HTTP (التي تهمنا في الـ ML Service)
            if (exception is HttpRequestException httpEx)
            {
                // لو الرد 422 أو أي 4xx، مستحيل نعيد المحاولة لأن المشكلة في الـ Request نفسه
                if (httpEx.StatusCode.HasValue &&
                    (int)httpEx.StatusCode >= 400 &&
                    (int)httpEx.StatusCode < 500)
                {
                    return false;
                }

                // أعد المحاولة فقط لو الخطأ 5xx (Server Error) أو مشكلة اتصال
                return true;
            }

            // 2. أخطاء الاتصال وقاعدة البيانات (Npgsql)
            if (exception is NpgsqlException ||
                exception is IOException ||
                exception is TimeoutException)
            {
                return true;
            }

            // 3. أي استثناء آخر (بما فيه الـ TaskCanceledException) لا تعيد المحاولة فيه
            return false;
        }

        private int GetRetryCountForRequest(string requestName)
        {
            return requestName switch
            {
                "GetProfileQuery" => 3,
                "GetActiveFleetQuery" => 3,
                "GetAllZonesQuery" => 3,
                "GetDemandForecastQuery" => 2,
                "LoginCommand" => 2,
                "RunOperationalSimulationCommand" => 1,
                _ => DefaultRetryCount
            };
        }
    }
}