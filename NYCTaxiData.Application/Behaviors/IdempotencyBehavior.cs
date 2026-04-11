using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Interfaces;
using System.Text.Json;

namespace NYCTaxiData.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for implementing idempotency.
    /// Prevents duplicate operations by caching responses based on idempotency keys.
    /// Protects against duplicate requests from network retries or accidental re-submissions.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    internal class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger;
        private readonly IIdempotencyService _idempotencyService;

        /// <summary>
        /// The header name for idempotency key in HTTP requests.
        /// </summary>
        private const string IdempotencyKeyHeader = "Idempotency-Key";

        /// <summary>
        /// Default cache expiration time (24 hours) for idempotency keys.
        /// </summary>
        private static readonly TimeSpan DefaultExpirationTime = TimeSpan.FromHours(24);

        /// <summary>
        /// Initializes a new instance of the <see cref="IdempotencyBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="idempotencyService">The idempotency service.</param>
        public IdempotencyBehavior(ILogger<IdempotencyBehavior<TRequest, TResponse>> logger, IIdempotencyService idempotencyService)
        {
            _logger = logger;
            _idempotencyService = idempotencyService;
        }

        /// <summary>
        /// Handles the request with idempotency check.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="next">The next request handler.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // Only apply idempotency to commands (requests that modify state)
            // Queries are naturally idempotent and don't need special handling
            if (!IsIdempotentRequest(requestName))
            {
                _logger.LogDebug("Request {RequestName} is not idempotent, skipping idempotency check", requestName);
                return await next();
            }

            // Try to extract idempotency key from request
            var idempotencyKey = ExtractIdempotencyKey(request);
            if (string.IsNullOrEmpty(idempotencyKey))
            {
                _logger.LogDebug("No idempotency key provided for {RequestName}, executing without idempotency", requestName);
                return await next();
            }

            _logger.LogDebug("Processing request {RequestName} with idempotency key {IdempotencyKey}", requestName, idempotencyKey);

            // Check if response is already cached
            var cachedResponse = await _idempotencyService.GetCachedResponseAsync(idempotencyKey, cancellationToken);
            if (cachedResponse != null)
            {
                _logger.LogInformation("Request {RequestName} with idempotency key {IdempotencyKey} returned cached response", requestName, idempotencyKey);

                try
                {
                    var deserializedResponse = JsonSerializer.Deserialize<TResponse>(cachedResponse);
                    return deserializedResponse ?? throw new InvalidOperationException("Failed to deserialize cached response");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing cached response for idempotency key {IdempotencyKey}", idempotencyKey);
                    throw;
                }
            }

            // Check if request is currently being processed (in-flight)
            if (await _idempotencyService.IsProcessingAsync(idempotencyKey, cancellationToken))
            {
                _logger.LogWarning("Request {RequestName} with idempotency key {IdempotencyKey} is already being processed", requestName, idempotencyKey);
                throw new InvalidOperationException($"Request with idempotency key {idempotencyKey} is already being processed");
            }

            // Mark request as processing
            await _idempotencyService.MarkAsProcessingAsync(idempotencyKey, cancellationToken);

            try
            {
                // Execute the request
                var response = await next();

                // Cache the successful response
                var serializedResponse = JsonSerializer.Serialize(response);
                await _idempotencyService.StoreCachedResponseAsync(
                    idempotencyKey,
                    serializedResponse,
                    DefaultExpirationTime,
                    cancellationToken);

                _logger.LogInformation("Request {RequestName} with idempotency key {IdempotencyKey} completed successfully", requestName, idempotencyKey);

                return response;
            }
            catch (Exception ex)
            {
                // Don't cache failed responses - allow retry
                _logger.LogError(ex, "Request {RequestName} with idempotency key {IdempotencyKey} failed with error: {ErrorMessage}", requestName, idempotencyKey, ex.Message);
                throw;
            }
            finally
            {
                // Clear the processing marker
                await _idempotencyService.ClearProcessingMarkerAsync(idempotencyKey, cancellationToken);
            }
        }

        /// <summary>
        /// Determines if a request should be idempotent.
        /// Idempotent requests are typically commands that modify state.
        /// </summary>
        /// <param name="requestName">The name of the request.</param>
        /// <returns>True if the request should be idempotent; otherwise, false.</returns>
        private bool IsIdempotentRequest(string requestName)
        {
            // Commands are idempotent (end with "Command")
            if (requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
                return true;

            // Queries are not idempotent (end with "Query")
            if (requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
                return false;

            // Default to non-idempotent
            return false;
        }

        /// <summary>
        /// Extracts the idempotency key from the request.
        /// Looks for an IdempotencyKey property on the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The idempotency key if found; null otherwise.</returns>
        private string? ExtractIdempotencyKey(TRequest request)
        {
            try
            {
                // Try to get IdempotencyKey property from request
                var idempotencyKeyProperty = typeof(TRequest).GetProperty("IdempotencyKey");
                if (idempotencyKeyProperty != null && idempotencyKeyProperty.CanRead)
                {
                    var value = idempotencyKeyProperty.GetValue(request);
                    if (value is string key && !string.IsNullOrEmpty(key))
                    {
                        return key;
                    }
                }

                // Try alternative names
                var keyProperty = typeof(TRequest).GetProperty("Key");
                if (keyProperty != null && keyProperty.CanRead)
                {
                    var value = keyProperty.GetValue(request);
                    if (value is string key && !string.IsNullOrEmpty(key))
                    {
                        return key;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting idempotency key from request {RequestType}", typeof(TRequest).Name);
                return null;
            }
        }
    }
}
