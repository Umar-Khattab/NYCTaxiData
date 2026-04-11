using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace NYCTaxiData.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for performance monitoring and alerting.
    /// Tracks request execution time, identifies performance degradation, and alerts on slow operations.
    /// Complements MetricsBehavior with real-time performance analysis and warnings.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    internal class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

        // Performance thresholds (in milliseconds)
        private const long SlowQueryThreshold = 500;        // Queries > 500ms are slow
        private const long SlowCommandThreshold = 1000;     // Commands > 1 second are slow
        private const long VerySlowThreshold = 5000;        // > 5 seconds is very slow

        // Thread-safe performance history tracking
        private static readonly object PerformanceLock = new object();
        private static readonly Dictionary<string, PerformanceHistory> PerformanceHistoryMap = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the request with performance monitoring.
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

                // Check performance and alert if necessary
                CheckPerformance(requestName, stopwatch.ElapsedMilliseconds);

                return response;
            }
            finally
            {
                stopwatch.Stop();
                // Record performance history
                RecordPerformanceHistory(requestName, stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Checks if request performance is acceptable and logs warnings if needed.
        /// </summary>
        /// <param name="requestName">The name of the request.</param>
        /// <param name="elapsedMilliseconds">The execution time in milliseconds.</param>
        private void CheckPerformance(string requestName, long elapsedMilliseconds)
        {
            var isQuery = requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase);
            var threshold = isQuery ? SlowQueryThreshold : SlowCommandThreshold;

            if (elapsedMilliseconds > VerySlowThreshold)
            {
                _logger.LogError(
                    "CRITICAL: Request {RequestName} is VERY SLOW - {ElapsedMilliseconds}ms (threshold: {Threshold}ms)",
                    requestName,
                    elapsedMilliseconds,
                    VerySlowThreshold);
            }
            else if (elapsedMilliseconds > threshold * 2)
            {
                _logger.LogWarning(
                    "WARNING: Request {RequestName} is significantly slow - {ElapsedMilliseconds}ms (threshold: {Threshold}ms)",
                    requestName,
                    elapsedMilliseconds,
                    threshold);
            }
            else if (elapsedMilliseconds > threshold)
            {
                _logger.LogWarning(
                    "SLOW: Request {RequestName} exceeded threshold - {ElapsedMilliseconds}ms (threshold: {Threshold}ms)",
                    requestName,
                    elapsedMilliseconds,
                    threshold);
            }
        }

        /// <summary>
        /// Records performance history for trend analysis.
        /// </summary>
        /// <param name="requestName">The name of the request.</param>
        /// <param name="elapsedMilliseconds">The execution time in milliseconds.</param>
        private void RecordPerformanceHistory(string requestName, long elapsedMilliseconds)
        {
            lock (PerformanceLock)
            {
                if (!PerformanceHistoryMap.TryGetValue(requestName, out var history))
                {
                    history = new PerformanceHistory { RequestName = requestName };
                    PerformanceHistoryMap[requestName] = history;
                }

                history.AddMeasurement(elapsedMilliseconds);

                // Check for performance degradation
                if (history.IsDegrading())
                {
                    var degradation = history.GetDegradationPercentage();
                    _logger.LogWarning(
                        "DEGRADATION: Request {RequestName} performance degraded by {DegradationPercent:F2}% ({PreviousAvg}ms → {CurrentAvg}ms)",
                        requestName,
                        degradation,
                        history.PreviousPeriodAverage,
                        history.CurrentPeriodAverage);
                }
            }
        }

        /// <summary>
        /// Gets all performance histories.
        /// </summary>
        /// <returns>Dictionary of performance histories.</returns>
        public static Dictionary<string, PerformanceHistory> GetAllPerformanceHistories()
        {
            lock (PerformanceLock)
            {
                return new Dictionary<string, PerformanceHistory>(PerformanceHistoryMap);
            }
        }

        /// <summary>
        /// Gets performance history for a specific request.
        /// </summary>
        /// <param name="requestName">The name of the request.</param>
        /// <returns>The performance history or null if not found.</returns>
        public static PerformanceHistory? GetPerformanceHistory(string requestName)
        {
            lock (PerformanceLock)
            {
                return PerformanceHistoryMap.TryGetValue(requestName, out var history) ? history : null;
            }
        }

        /// <summary>
        /// Gets all slow operations (currently exceeding threshold).
        /// </summary>
        /// <returns>List of slow operations.</returns>
        public static List<(string RequestName, long AverageTime, long MaxTime)> GetSlowOperations()
        {
            lock (PerformanceLock)
            {
                var slowOps = new List<(string, long, long)>();

                foreach (var history in PerformanceHistoryMap.Values)
                {
                    var isQuery = history.RequestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase);
                    var threshold = isQuery ? SlowQueryThreshold : SlowCommandThreshold;

                    if (history.CurrentPeriodAverage > threshold)
                    {
                        slowOps.Add((history.RequestName, history.CurrentPeriodAverage, history.MaxExecutionTime));
                    }
                }

                return slowOps.OrderByDescending(x => x.Item2).ToList();
            }
        }

        /// <summary>
        /// Gets all degrading operations.
        /// </summary>
        /// <returns>List of degrading operations with degradation percentage.</returns>
        public static List<(string RequestName, double DegradationPercent)> GetDegradingOperations()
        {
            lock (PerformanceLock)
            {
                var degradingOps = new List<(string, double)>();

                foreach (var history in PerformanceHistoryMap.Values)
                {
                    if (history.IsDegrading())
                    {
                        degradingOps.Add((history.RequestName, history.GetDegradationPercentage()));
                    }
                }

                return degradingOps.OrderByDescending(x => x.Item2).ToList();
            }
        }

        /// <summary>
        /// Resets all performance histories.
        /// </summary>
        public static void ResetPerformanceHistories()
        {
            lock (PerformanceLock)
            {
                PerformanceHistoryMap.Clear();
            }
        }
    }

    /// <summary>
    /// Tracks performance history for a request type with trend analysis.
    /// </summary>
    public class PerformanceHistory
    {
        /// <summary>
        /// The name of the request.
        /// </summary>
        public string RequestName { get; set; } = string.Empty;

        /// <summary>
        /// Recent measurements for current period analysis (last 100 requests).
        /// </summary>
        private readonly Queue<long> RecentMeasurements = new(100);

        /// <summary>
        /// Previous period average for trend detection.
        /// </summary>
        public long PreviousPeriodAverage { get; private set; }

        /// <summary>
        /// Current period average.
        /// </summary>
        public long CurrentPeriodAverage => RecentMeasurements.Count > 0 ? (long)RecentMeasurements.Average() : 0;

        /// <summary>
        /// Minimum execution time recorded.
        /// </summary>
        public long MinExecutionTime { get; private set; } = long.MaxValue;

        /// <summary>
        /// Maximum execution time recorded.
        /// </summary>
        public long MaxExecutionTime { get; private set; }

        /// <summary>
        /// Total number of measurements.
        /// </summary>
        public long TotalMeasurements { get; private set; }

        /// <summary>
        /// Adds a performance measurement.
        /// </summary>
        /// <param name="executionTime">The execution time in milliseconds.</param>
        public void AddMeasurement(long executionTime)
        {
            RecentMeasurements.Enqueue(executionTime);
            if (RecentMeasurements.Count > 100)
                RecentMeasurements.Dequeue();

            TotalMeasurements++;
            MinExecutionTime = Math.Min(MinExecutionTime, executionTime);
            MaxExecutionTime = Math.Max(MaxExecutionTime, executionTime);

            // Update previous period average every 100 measurements
            if (TotalMeasurements % 100 == 0)
                PreviousPeriodAverage = CurrentPeriodAverage;
        }

        /// <summary>
        /// Determines if performance is degrading compared to previous period.
        /// </summary>
        /// <returns>True if performance degradation detected.</returns>
        public bool IsDegrading()
        {
            if (PreviousPeriodAverage == 0 || CurrentPeriodAverage == 0)
                return false;

            var degradation = ((CurrentPeriodAverage - PreviousPeriodAverage) / (double)PreviousPeriodAverage) * 100;
            return degradation > 20; // 20% degradation threshold
        }

        /// <summary>
        /// Gets the performance degradation percentage.
        /// </summary>
        /// <returns>The degradation percentage (positive = slower).</returns>
        public double GetDegradationPercentage()
        {
            if (PreviousPeriodAverage == 0)
                return 0;

            return ((CurrentPeriodAverage - PreviousPeriodAverage) / (double)PreviousPeriodAverage) * 100;
        }

        /// <summary>
        /// Gets a summary of the performance history.
        /// </summary>
        /// <returns>A formatted string summary.</returns>
        public override string ToString()
        {
            var degradation = IsDegrading() ? GetDegradationPercentage() : 0;

            return $@"
Request: {RequestName}
  Measurements: {TotalMeasurements}
  
Performance:
  Current Avg: {CurrentPeriodAverage}ms
  Previous Avg: {PreviousPeriodAverage}ms
  Min:         {(MinExecutionTime == long.MaxValue ? 0 : MinExecutionTime)}ms
  Max:         {MaxExecutionTime}ms
  
Trend:
  Degradation: {(IsDegrading() ? $"{degradation:F2}%" : "None")}"
            ;
        }
    }
}
