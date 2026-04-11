using MediatR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;

namespace NYCTaxiData.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for enforcing timeouts on request execution.
    /// Uses Polly library to implement timeout policies with configurable durations.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    internal class TimeoutBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<TimeoutBehavior<TRequest, TResponse>> _logger;

        /// <summary>
        /// Default timeout duration in seconds. Can be overridden per request type.
        /// </summary>
        private const int DefaultTimeoutSeconds = 30;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public TimeoutBehavior(ILogger<TimeoutBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the request with a timeout policy.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="next">The next request handler.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the request exceeds the timeout duration.</exception>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var timeoutSeconds = GetTimeoutForRequest(requestName);

            _logger.LogDebug(
                "Request {RequestName} will be executed with timeout of {TimeoutSeconds} seconds",
                requestName,
                timeoutSeconds);

            try
            {
                // Create an async timeout policy
                var timeoutPolicy = Policy.TimeoutAsync<TResponse>(
                    TimeSpan.FromSeconds(timeoutSeconds),
                    TimeoutStrategy.Optimistic);

                // Execute the request with the timeout policy
                var response = await timeoutPolicy.ExecuteAsync(
                    async ct => await next(),
                    cancellationToken);

                return response;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "Request {RequestName} exceeded timeout of {TimeoutSeconds} seconds",
                    requestName,
                    timeoutSeconds);

                throw;
            }
        }

        /// <summary>
        /// Gets the timeout duration for a specific request type.
        /// Can be overridden to provide custom timeouts for specific requests.
        /// </summary>
        /// <param name="requestName">The name of the request.</param>
        /// <returns>The timeout duration in seconds.</returns>
        private int GetTimeoutForRequest(string requestName)
        {
            // Define custom timeouts for specific requests
            return requestName switch
            {
                // Queries - shorter timeout
                "GetProfileQuery" => 10,
                "GetActiveFleetQuery" => 15,
                "GetAllZonesQuery" => 15,
                "GetTopLevelKpisQuery" => 20,
                "GetDemandForecastQuery" => 25,
                "GetLiveDispatchFeedQuery" => 15,
                "GetSpecificZoneInsightsQuery" => 15,
                "GetDemandVelocityChartQuery" => 20,
                "GetSystemThresholdsQuery" => 10,
                "GetTripHistoryQuery" => 20,
                "GetShiftStatisticsQuery" => 15,
                "GetOptimalDriverScheduleQuery" => 40,
                "GetExplainableAiInsightQuery" => 45,
                "GetDispatchRecommendationQuery" => 30,

                // Commands - longer timeout
                "LoginCommand" => 15,
                "RegisterCommand" => 20,
                "RefreshTokenCommand" => 10,
                "SendOtpCommand" => 15,
                "VerifyOtpCommand" => 15,
                "ResetPasswordCommand" => 20,
                "UpdateSystemThresholdsCommand" => 30,
                "StartTripCommand" => 20,
                "EndTripCommand" => 25,
                "ManualDispatchCommand" => 20,
                "UpdateDriverStatusCommand" => 15,
                "SyncOfflineTripsCommand" => 30,

                // AI operations - longest timeout
                "RunOperationalSimulationCommand" => 60,
                "RunStrategicSimulationCommand" => 90,
                "TriggerModelRetrainingCommand" => 120,
                "ProcessVoiceAssistantQuery" => 45,

                // Default
                _ => DefaultTimeoutSeconds
            };
        }
    }
}
