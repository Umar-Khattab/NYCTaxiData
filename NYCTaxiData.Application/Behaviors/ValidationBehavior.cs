using FluentValidation;
using MediatR;
using ValidationException = NYCTaxiData.Application.Common.Exceptions.ValidationException;

namespace NYCTaxiData.Application.Behaviors;

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
        // 1. لو مفيش فالدتورز كمل الطريق عادي
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        // 2. التنفيذ Async عشان Performance المشروع (UrbanFlow)
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // 3. تجميع الأخطاء بشكل مختصر (زي ما إنت طلبت)
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // 4. لو فيه أخطاء، ارمي الـ Exception الموحد بتاعنا
        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}