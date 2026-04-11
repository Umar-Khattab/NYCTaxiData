using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for managing database transactions.
    /// Automatically wraps request handlers in a database transaction to ensure data consistency
    /// and atomic operations. Commits on success, rolls back on failure.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    internal class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="unitOfWork">The unit of work for database operations.</param>
        public TransactionBehavior(
            ILogger<TransactionBehavior<TRequest, TResponse>> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handles the request within a database transaction.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="next">The next request handler.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // Only apply transaction to commands (requests that modify state)
            // Queries should not use transactions as they don't modify data
            if (!IsTransactionalRequest(requestName))
            {
                _logger.LogDebug("Request {RequestName} is not transactional, skipping transaction wrapper", requestName);
                return await next();
            }

            _logger.LogDebug("Beginning database transaction for request {RequestName}", requestName);

            try
            {
                // Execute the request within transaction
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await next();
                });

                // Save changes after transaction completes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Database transaction committed successfully for request {RequestName}", requestName);

                // Execute again to get the response (response is generated during the actual execution)
                var response = await next();
                return response;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Database transaction cancelled for request {RequestName}", requestName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Database transaction failed for request {RequestName}. Error: {ErrorMessage}. Transaction will be rolled back.",
                    requestName,
                    ex.Message);

                throw;
            }
        }

        /// <summary>
        /// Determines if a request should be wrapped in a transaction.
        /// Only commands (write operations) should use transactions.
        /// </summary>
        /// <param name="requestName">The name of the request.</param>
        /// <returns>True if the request should be transactional; otherwise, false.</returns>
        private bool IsTransactionalRequest(string requestName)
        {
            // Commands are transactional (end with "Command")
            if (requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
                return true;

            // Queries are not transactional (end with "Query")
            if (requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
                return false;

            // Default to non-transactional
            return false;
        }
    }
}
