using MediatR;
using NYCTaxiData.Application.Common.Attributes;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;

namespace NYCTaxiData.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for authorization.
    /// Checks if the current user has the required permissions to execute a command or query.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    internal class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ICurrentUserService _currentUserService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="currentUserService">The current user service.</param>
        public AuthorizationBehavior(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Handles the authorization check before executing the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="next">The next request handler.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        /// <exception cref="UnauthorizedException">Thrown when the user is not authenticated or does not have the required role.</exception>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var authorizeAttribute = request.GetType()
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
                .FirstOrDefault() as AuthorizeAttribute;

            // If no [Authorize] attribute is present, skip authorization check
            if (authorizeAttribute == null)
            {
                return await next();
            }

            // Check if user is authenticated
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            {
                throw new UnauthorizedException("User is not authenticated.");
            }

            // If specific roles are required, check if user has one of them
            if (authorizeAttribute.Roles.Length > 0)
            {
                var hasRequiredRole = authorizeAttribute.Roles.Contains(_currentUserService.UserRole ?? default);

                if (!hasRequiredRole)
                {
                    var requiredRoles = string.Join(", ", authorizeAttribute.Roles.Select(r => r.ToString()));
                    throw new UnauthorizedException(
                        $"User does not have the required role(s) to execute this action. Required roles: {requiredRoles}");
                }
            }

            // User is authorized, proceed with the request
            return await next();
        }
    }
}
