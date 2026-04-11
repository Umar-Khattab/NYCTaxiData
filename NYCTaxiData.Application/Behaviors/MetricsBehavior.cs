using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace NYCTaxiData.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for collecting performance and operational metrics.
    /// Tracks execution time, success/failure rates, request counts, and other observability data.
    /// Essential for monitoring, alerting, and performance optimization.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    internal class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<MetricsBehavior<TRequest, TResponse>> _logger;

        // Thread-safe metrics storage
        private static readonly object MetricsLock = new object();
        private static readonly Dictionary<string, RequestMetrics> RequestMetricsMap = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public MetricsBehavior(ILogger<MetricsBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the request and collects metrics.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="next">The next request handler.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await next();
                stopwatch.Stop();

                // Record successful execution
                RecordMetrics(requestName, stopwatch.ElapsedMilliseconds, success: true, error: null);

                _logger.LogInformation(
                    "Request {RequestName} completed in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (OperationCanceledException ex)
            {
                stopwatch.Stop();
                RecordMetrics(requestName, stopwatch.ElapsedMilliseconds, success: false, error: "Cancelled");

                _logger.LogWarning(
                    "Request {RequestName} was cancelled after {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var errorType = ex.GetType().Name;
                RecordMetrics(requestName, stopwatch.ElapsedMilliseconds, success: false, error: errorType);

                _logger.LogError(
                    "Request {RequestName} failed after {ElapsedMilliseconds}ms with error: {ErrorType}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    errorType);

                throw;
            }
        }

        /// <summary>
        /// Records metrics for a request.
        /// </summary>
        /// <param name="requestName">The name of the request.</param>
        /// <param name="elapsedMilliseconds">The execution time in milliseconds.</param>
        /// <param name="success">Whether the request was successful.</param>
        /// <param name="error">The error type if failed.</param>
        private void RecordMetrics(string requestName, long elapsedMilliseconds, bool success, string? error)
        {
            lock (MetricsLock)
            {
                if (!RequestMetricsMap.TryGetValue(requestName, out var metrics))
                {
                    metrics = new RequestMetrics { RequestName = requestName };
                    RequestMetricsMap[requestName] = metrics;
                }

                metrics.TotalRequests++;
                metrics.TotalExecutionTime += elapsedMilliseconds;

                if (success)
                {
                    metrics.SuccessfulRequests++;
                }
                else
                {
                    metrics.FailedRequests++;
                    if (!string.IsNullOrEmpty(error))
                    {
                        if (!metrics.ErrorCounts.ContainsKey(error))
                            metrics.ErrorCounts[error] = 0;
                        metrics.ErrorCounts[error]++;
                    }
                }

                // Update min/max/average
                if (elapsedMilliseconds < metrics.MinExecutionTime || metrics.MinExecutionTime == 0)
                    metrics.MinExecutionTime = elapsedMilliseconds;

                if (elapsedMilliseconds > metrics.MaxExecutionTime)
                    metrics.MaxExecutionTime = elapsedMilliseconds;

                metrics.AverageExecutionTime = metrics.TotalExecutionTime / metrics.TotalRequests;
            }
        }

        /// <summary>
        /// Gets all collected metrics.
        /// </summary>
        /// <returns>Dictionary of request metrics.</returns>
        public static Dictionary<string, RequestMetrics> GetAllMetrics()
        {
            lock (MetricsLock)
            {
                return new Dictionary<string, RequestMetrics>(RequestMetricsMap);
            }
        }

        /// <summary>
        /// Gets metrics for a specific request type.
        /// </summary>
        /// <param name="requestName">The name of the request.</param>
        /// <returns>The metrics or null if not found.</returns>
        public static RequestMetrics? GetMetrics(string requestName)
        {
            lock (MetricsLock)
            {
                return RequestMetricsMap.TryGetValue(requestName, out var metrics) ? metrics : null;
            }
        }

        /// <summary>
        /// Resets all metrics.
        /// </summary>
        public static void ResetMetrics()
        {
            lock (MetricsLock)
            {
                RequestMetricsMap.Clear();
            }
        }
    }

    /// <summary>
    /// Container for request metrics.
    /// </summary>
    public class RequestMetrics
    {
        /// <summary>
        /// The name of the request.
        /// </summary>
        public string RequestName { get; set; } = string.Empty;

        /// <summary>
        /// Total number of requests.
        /// </summary>
        public long TotalRequests { get; set; }

        /// <summary>
        /// Total number of successful requests.
        /// </summary>
        public long SuccessfulRequests { get; set; }

        /// <summary>
        /// Total number of failed requests.
        /// </summary>
        public long FailedRequests { get; set; }

        /// <summary>
        /// Success rate as a percentage (0-100).
        /// </summary>
        public decimal SuccessRate =>
            TotalRequests > 0 ? (SuccessfulRequests * 100m / TotalRequests) : 0;

        /// <summary>
        /// Total execution time in milliseconds.
        /// </summary>
        public long TotalExecutionTime { get; set; }

        /// <summary>
        /// Minimum execution time in milliseconds.
        /// </summary>
        public long MinExecutionTime { get; set; }

        /// <summary>
        /// Maximum execution time in milliseconds.
        /// </summary>
        public long MaxExecutionTime { get; set; }

        /// <summary>
        /// Average execution time in milliseconds.
        /// </summary>
        public long AverageExecutionTime { get; set; }

        /// <summary>
        /// Count of errors by type.
        /// </summary>
        public Dictionary<string, int> ErrorCounts { get; set; } = new();

        /// <summary>
        /// Gets the most common error type.
        /// </summary>
        public string? MostCommonError =>
            ErrorCounts.OrderByDescending(x => x.Value).FirstOrDefault().Key;

        /// <summary>
        /// Gets the most common error count.
        /// </summary>
        public int MostCommonErrorCount =>
            ErrorCounts.OrderByDescending(x => x.Value).FirstOrDefault().Value;

        /// <summary>
        /// Gets a summary of the metrics.
        /// </summary>
        /// <returns>A formatted string summary.</returns>
        public override string ToString()
        {
            return $@"
Request: {RequestName}
  Total:     {TotalRequests}
  Success:   {SuccessfulRequests} ({SuccessRate:F2}%)
  Failed:    {FailedRequests}

Performance:
  Min:       {MinExecutionTime}ms
  Max:       {MaxExecutionTime}ms
  Avg:       {AverageExecutionTime}ms

Errors: {(ErrorCounts.Count > 0 ? string.Join(", ", ErrorCounts.Select(x => $"{x.Key}:{x.Value}")) : "None")}";
        }
    }
}
