using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace NYCTaxiData.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for caching query results.
    /// Improves performance by caching responses from idempotent queries.
    /// Automatically handles cache invalidation through request attributes.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    internal class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
        private readonly IDistributedCache _cache;

        /// <summary>
        /// Default cache duration (5 minutes) for cached query results.
        /// </summary>
        private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="cache">The distributed cache instance.</param>
        public CachingBehavior(ILogger<CachingBehavior<TRequest, TResponse>> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Handles the request with caching logic.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="next">The next request handler.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // Only apply caching to queries (read-only operations)
            if (!IsCacheableRequest(requestName))
            {
                _logger.LogDebug("Request {RequestName} is not cacheable, skipping cache", requestName);
                return await next();
            }

            // Generate cache key
            var cacheKey = GenerateCacheKey(requestName, request);
            if (string.IsNullOrEmpty(cacheKey))
            {
                _logger.LogDebug("Could not generate cache key for {RequestName}, executing without cache", requestName);
                return await next();
            }

            _logger.LogDebug("Checking cache for request {RequestName} with key {CacheKey}", requestName, cacheKey);

            try
            {
                // Try to get from cache
                var cachedResponse = await _cache.GetStringAsync(cacheKey, cancellationToken);
                if (!string.IsNullOrEmpty(cachedResponse))
                {
                    _logger.LogInformation("Cache hit for request {RequestName}", requestName);

                    try
                    {
                        var deserializedResponse = JsonSerializer.Deserialize<TResponse>(cachedResponse);
                        return deserializedResponse ?? throw new InvalidOperationException("Failed to deserialize cached response");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deserializing cached response for request {RequestName}", requestName);
                        // Fall through and execute normally
                    }
                }

                _logger.LogDebug("Cache miss for request {RequestName}", requestName);

                // Cache miss or deserialization failed - execute handler
                var response = await next();

                // Cache the response
                var cacheDuration = GetCacheDuration(requestName);
                var serializedResponse = JsonSerializer.Serialize(response);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheDuration
                };

                await _cache.SetStringAsync(cacheKey, serializedResponse, cacheOptions, cancellationToken);
                _logger.LogInformation("Cached response for request {RequestName} for {CacheDuration} seconds", requestName, cacheDuration.TotalSeconds);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during caching for request {RequestName}", requestName);
                // On error, just execute normally without caching
                return await next();
            }
        }

        /// <summary>
        /// Determines if a request should be cached.
        /// Only queries (read-only operations) should be cached.
        /// </summary>
        /// <param name="requestName">The name of the request.</param>
        /// <returns>True if the request should be cached; otherwise, false.</returns>
        private bool IsCacheableRequest(string requestName)
        {
            // Queries are cacheable (end with "Query")
            if (requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
                return true;

            // Commands are not cacheable (end with "Command")
            if (requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
                return false;

            // Default to non-cacheable
            return false;
        }

        /// <summary>
        /// Generates a unique cache key based on request type and parameters.
        /// </summary>
        /// <param name="requestName">The name of the request.</param>
        /// <param name="request">The request object.</param>
        /// <returns>The generated cache key.</returns>
        private string GenerateCacheKey(string requestName, TRequest request)
        {
            try
            {
                // Serialize the entire request to create a unique key
                var requestJson = JsonSerializer.Serialize(request);
                var hash = GetHashCode(requestJson);
                return $"cache:{requestName}:{hash}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error generating cache key for request {RequestName}", requestName);
                return null!;
            }
        }

        /// <summary>
        /// Gets a stable hash code for the given string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The hash code as a string.</returns>
        private string GetHashCode(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return Convert.ToHexString(hashedBytes)[..16]; // First 16 characters
            }
        }

        /// <summary>
        /// Gets the cache duration for a specific request type.
        /// Can be customized per query for different cache lifetimes.
        /// </summary>
        /// <param name="requestName">The name of the request.</param>
        /// <returns>The cache duration.</returns>
        private TimeSpan GetCacheDuration(string requestName)
        {
            return requestName switch
            {
                // Long-lived cache (30 minutes) - relatively static data
                "GetTopLevelKpisQuery" => TimeSpan.FromMinutes(30),
                "GetAllZonesQuery" => TimeSpan.FromMinutes(30),
                "GetSystemThresholdsQuery" => TimeSpan.FromMinutes(30),
                "GetOptimalDriverScheduleQuery" => TimeSpan.FromMinutes(20),

                // Medium cache (10 minutes) - semi-static data
                "GetActiveFleetQuery" => TimeSpan.FromMinutes(10),
                "GetDemandForecastQuery" => TimeSpan.FromMinutes(10),
                "GetShiftStatisticsQuery" => TimeSpan.FromMinutes(10),
                "GetDemandVelocityChartQuery" => TimeSpan.FromMinutes(10),

                // Short-lived cache (2 minutes) - frequently changing data
                "GetLiveDispatchFeedQuery" => TimeSpan.FromMinutes(2),
                "GetProfileQuery" => TimeSpan.FromMinutes(5),
                "GetSpecificZoneInsightsQuery" => TimeSpan.FromMinutes(3),
                "GetTripHistoryQuery" => TimeSpan.FromMinutes(2),

                // Very short cache (30 seconds) - real-time data
                "GetExplainableAiInsightQuery" => TimeSpan.FromSeconds(30),
                "GetDispatchRecommendationQuery" => TimeSpan.FromSeconds(45),

                // Default
                _ => DefaultCacheDuration
            };
        }
    }
}
