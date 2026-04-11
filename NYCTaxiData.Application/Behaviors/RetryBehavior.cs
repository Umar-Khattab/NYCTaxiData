using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Exceptions;
using Polly;
using Polly.Retry;

namespace NYCTaxiData.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for implementing retry logic on transient failures.
    /// Uses Polly library to automatically retry failed requests with exponential backoff.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    internal class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;

        /// <summary>
        /// Default number of retry attempts (3 retries = 4 total attempts).
        /// </summary>
        private const int DefaultRetryCount = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public RetryBehavior(ILogger<RetryBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the request with retry policy for transient failures.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="next">The next request handler.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var retryCount = GetRetryCountForRequest(requestName);

            // Create retry policy with exponential backoff
            var retryPolicy = Policy
                .Handle<Exception>(IsTransientError)
                .OrResult<TResponse>(r => false)  // Don't retry on successful result
                .WaitAndRetryAsync<TResponse>(
                    retryCount: retryCount,
                    sleepDurationProvider: retryAttempt =>
                    {
                        // Exponential backoff: 1s, 2s, 4s, 8s, etc.
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1));
                        return delay;
                    },
                    onRetry: (outcome, timespan, retryNumber, context) =>
                    {
                        _logger.LogWarning(
                            "Request {RequestName} failed with exception {ExceptionType}: {Message}. Retrying in {RetryDelay}ms (Attempt {RetryNumber}/{RetryCount})",
                            requestName,
                            outcome.Exception?.GetType().Name,
                            outcome.Exception?.Message,
                            (long)timespan.TotalMilliseconds,
                            retryNumber,
                            retryCount);
                    });

            _logger.LogDebug(
                "Request {RequestName} will be executed with retry policy (max {RetryCount} retries)",
                requestName,
                retryCount);

            try
            {
                var response = await retryPolicy.ExecuteAsync(async ct => await next(), cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Request {RequestName} failed after {RetryCount} retries. Final error: {ErrorMessage}",
                    requestName,
                    retryCount,
                    ex.Message);

                throw;
            }
        }

        /// <summary>
        /// Determines if an exception is transient and should trigger a retry.
        /// Transient errors include database connection issues, network timeouts, etc.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns>True if the exception is transient; otherwise, false.</returns>
        private bool IsTransientError(Exception exception)
        {
            // Don't retry on validation or authorization errors
            if (exception is ValidationException || exception is UnauthorizedException)
                return false;

            // Don't retry on operation cancelled
            if (exception is OperationCanceledException)
                return false;

            // Retry on database connection errors
            if (exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                exception.Message.Contains("temporarily unavailable", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Retry on SQL-specific errors
            if (exception.InnerException?.GetType().Name.Contains("SqlException") == true ||
                exception.InnerException?.GetType().Name.Contains("NpgsqlException") == true)
            {
                return true;
            }

            // Retry on HTTP request exceptions (network issues)
            if (exception.GetType().Name.Contains("HttpRequestException"))
                return true;

            // Retry on IO exceptions
            if (exception is IOException or TimeoutException)
                return true;

            // Don't retry on other exceptions (business logic errors)
            return false;
        }

        /// <summary>
        /// Gets the retry count for a specific request type.
        /// Can be overridden to provide custom retry logic for specific requests.
        /// </summary>
        /// <param name="requestName">The name of the request.</param>
        /// <returns>The number of retry attempts.</returns>
        private int GetRetryCountForRequest(string requestName)
        {
            // Define custom retry counts for specific requests
            return requestName switch
            {
                // Queries - can retry safely (idempotent)
                "GetProfileQuery" => 3,
                "GetActiveFleetQuery" => 3,
                "GetAllZonesQuery" => 3,
                "GetTopLevelKpisQuery" => 2,
                "GetDemandForecastQuery" => 2,
                "GetLiveDispatchFeedQuery" => 3,

                // Database operations - retry on transient failures
                "LoginCommand" => 2,
                "RegisterCommand" => 2,
                "UpdateSystemThresholdsCommand" => 2,
                "UpdateDriverStatusCommand" => 2,
                "SyncOfflineTripsCommand" => 3,

                // External integrations - higher retry count
                "SendOtpCommand" => 3,
                "ProcessVoiceAssistantQuery" => 2,

                // Long-running operations - lower retry count (already take time)
                "RunOperationalSimulationCommand" => 1,
                "RunStrategicSimulationCommand" => 1,
                "TriggerModelRetrainingCommand" => 1,

                // Default
                _ => DefaultRetryCount
            };
        }
    }
}
