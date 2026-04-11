using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NYCTaxiData.Application.Behaviors;
using System.Reflection;

namespace NYCTaxiData.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register all Validators automatically
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Register MediatR and Pipeline Behaviors
            services.AddMediatR(config =>
            {
                // Register all Handlers and Commands/Queries automatically
                config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

                // Register Pipeline Behaviors (Order is STRICTLY important)
                // 1. Metrics - collect performance/operational metrics
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));

                // 2. Performance - detect slow requests / degradation
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

                // 3. Logging - request tracing and structured logs
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

                // 4. Caching - short-circuit queries with cached results
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

                // 5. Validation - validate requests via FluentValidation
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

                // 6. Authorization - enforce permissions/roles
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

                // 7. Idempotency - prevent duplicate side-effecting operations
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));

                // 8. Retry - retry transient failures where appropriate
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));

                // 9. Timeout - enforce request time limits and cancel
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TimeoutBehavior<,>));

                // 10. Exception Handling - centralized exception categorization
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

                // 11. Transaction - manage DB transactions (inner-most)
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            });

            return services;
        }
    }
}
