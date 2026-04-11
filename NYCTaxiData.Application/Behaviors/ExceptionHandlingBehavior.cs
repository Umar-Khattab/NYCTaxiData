using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Exceptions;

namespace NYCTaxiData.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for centralized exception handling.
    /// Catches exceptions from the request pipeline, logs them appropriately,
    /// and prevents uncaught exceptions from propagating. Works with middleware
    /// for comprehensive error handling across the application.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    internal class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the request with exception handling.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="next">The next request handler.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        /// <exception cref="Exception">Re-throws the exception after logging.</exception>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            try
            {
                return await next();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(
                    "Validation exception for request {RequestName}. Errors: {Errors}",
                    requestName,
                    string.Join("; ", ex.Errors.SelectMany(x => x.Value)));

                throw;
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt for request {RequestName}. User: {UserId}",
                    requestName,
                    ex.UserId);

                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(
                    "Resource not found for request {RequestName}. Resource: {ResourceType}",
                    requestName,
                    ex.ResourceType);

                throw;
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning(
                    "Conflict for request {RequestName}. Details: {Message}",
                    requestName,
                    ex.Message);

                throw;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Operation cancelled for request {RequestName}", requestName);
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Timeout occurred for request {RequestName}", requestName);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation for request {RequestName}", requestName);
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument for request {RequestName}", requestName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception for request {RequestName}. Exception type: {ExceptionType}. Message: {Message}",
                    requestName,
                    ex.GetType().Name,
                    ex.Message);

                throw;
            }
        }
    }

    /// <summary>
    /// Custom exception for not found resources.
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Gets the type of resource that was not found.
        /// </summary>
        public string ResourceType { get; }

        /// <summary>
        /// Gets the identifier of the resource.
        /// </summary>
        public string ResourceId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        /// <param name="resourceType">The type of resource not found.</param>
        /// <param name="resourceId">The identifier of the resource.</param>
        public NotFoundException(string resourceType, string resourceId)
            : base($"{resourceType} with id {resourceId} was not found.")
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }
    }

    /// <summary>
    /// Custom exception for unauthorized access attempts.
    /// </summary>
    public class UnauthorizedException : Exception
    {
        /// <summary>
        /// Gets the user ID that attempted unauthorized access.
        /// </summary>
        public string? UserId { get; }

        /// <summary>
        /// Gets the reason for the unauthorized access.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="reason">The reason for unauthorized access.</param>
        /// <param name="userId">The user ID attempting access.</param>
        public UnauthorizedException(string message, string reason = "Unauthorized", string? userId = null)
            : base(message)
        {
            Reason = reason;
            UserId = userId;
        }
    }

    /// <summary>
    /// Custom exception for conflict situations (e.g., duplicate resources).
    /// </summary>
    public class ConflictException : Exception
    {
        /// <summary>
        /// Gets the resource type involved in the conflict.
        /// </summary>
        public string ResourceType { get; }

        /// <summary>
        /// Gets the identifier of the conflicting resource.
        /// </summary>
        public string ResourceId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="resourceType">The type of resource involved.</param>
        /// <param name="resourceId">The identifier of the resource.</param>
        public ConflictException(string message, string resourceType, string resourceId)
            : base(message)
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }
    }
}
