using FluentValidation;
using MediatR;
using NYCTaxiData.Application.Common.Plumping; // تأكد من الـ Spelling المعتمد للـ Result (Plumbing أو Plumping)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ValidationException = NYCTaxiData.Application.Common.Exceptions.ValidationException;

namespace NYCTaxiData.Application.Behaviors
{
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
            // 1. لو مفيش فالديتورز كمل الطريق عادي
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            // 2. التنفيذ Async لأعلى أداء واستقرار للنظام
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            // 3. تجميع الأخطاء بشكل مختصر ونظيف
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            // 4. 🚀 التطوير السحري: لو فيه أخطاء، نمنع الـ Crash ونرجع الـ Failure الموحد فوراً
            if (failures.Any())
            {
                var errorMessages = string.Join("; ", failures.Select(f => f.ErrorMessage));

                // Try to find a public static Failure(string message, string code) method on TResponse
                var failureMethod = typeof(TResponse)
                    .GetMethod("Failure", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(string) }, null);

                // If not found on the constructed type, try its generic type definition (covers some generic static factories)
                if (failureMethod == null && typeof(TResponse).IsGenericType)
                {
                    var genericDef = typeof(TResponse).GetGenericTypeDefinition();
                    failureMethod = genericDef.GetMethod("Failure", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(string) }, null);

                    // If the method is on the generic definition, bind it to the constructed type
                    if (failureMethod != null)
                    {
                        var constructedType = genericDef.MakeGenericType(typeof(TResponse).GetGenericArguments());
                        failureMethod = constructedType.GetMethod("Failure", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(string) }, null);
                    }
                }

                if (failureMethod != null)
                {
                    var failureResult = failureMethod.Invoke(null, new object[] { errorMessages, "ValidationError" });
                    return (TResponse)failureResult!;
                }

                // Fallback: throw for other response types
                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}