namespace NYCTaxiData.Application.Common.Interfaces
{
    /// <summary>
    /// Service for managing idempotency keys and cached responses.
    /// Prevents duplicate operations by storing and retrieving cached responses.
    /// </summary>
    public interface IIdempotencyService
    {
        /// <summary>
        /// Checks if a response is already cached for the given idempotency key.
        /// </summary>
        /// <param name="idempotencyKey">The unique idempotency key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The cached response if found; null otherwise.</returns>
        Task<string?> GetCachedResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores a response for the given idempotency key.
        /// </summary>
        /// <param name="idempotencyKey">The unique idempotency key.</param>
        /// <param name="response">The serialized response to cache.</param>
        /// <param name="expirationTime">How long to keep the cache (default: 24 hours).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StoreCachedResponseAsync(
            string idempotencyKey,
            string response,
            TimeSpan? expirationTime = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a cached response for the given idempotency key.
        /// </summary>
        /// <param name="idempotencyKey">The unique idempotency key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveCachedResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if an idempotency key is currently being processed (in-flight).
        /// </summary>
        /// <param name="idempotencyKey">The unique idempotency key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the request is currently being processed; otherwise, false.</returns>
        Task<bool> IsProcessingAsync(string idempotencyKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks an idempotency key as being processed.
        /// </summary>
        /// <param name="idempotencyKey">The unique idempotency key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MarkAsProcessingAsync(string idempotencyKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears the in-flight processing marker for an idempotency key.
        /// </summary>
        /// <param name="idempotencyKey">The unique idempotency key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ClearProcessingMarkerAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    }
}
