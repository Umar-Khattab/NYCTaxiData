using FluentValidation;
using MediatR;
using NYCTaxiData.Application.Common.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValidationException = NYCTaxiData.Application.Common.Exceptions.ValidationException;

namespace NYCTaxiData.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior that runs all registered FluentValidation validators
    /// for the incoming request. Throws <see cref="ValidationException"/> on failure,
    /// which is caught and converted to a 400 response by <see cref="ExceptionHandlingBehavior{TRequest,TResponse}"/>.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
                throw new ValidationException(failures);

            return await next();
        }
    }
}