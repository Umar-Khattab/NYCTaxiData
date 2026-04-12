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

                // =======================================================================
                // PIPELINE BEHAVIORS (Order is STRICTLY important - Outermost to Innermost)
                // =======================================================================

                // 1. Exception Handling (Outermost to catch exceptions from ANY inner behavior)
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

                // 2. Telemetry & Observability
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

                // 3. Security FIRST (Gatekeeper)
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

                // 4. Validation (Bouncer - check data only for authorized users)
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

                // 5. Caching (Return fast, but ONLY for authorized & valid requests)
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

                // 6. Idempotency (Ensure we don't process duplicate side-effects)
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));

                // 7. Resilience (Retry & Timeout wrapping the actual database execution)
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TimeoutBehavior<,>));

                // 8. Transaction (Innermost - ensures atomic DB operations right before the Handler)
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            });

            return services;
        }
    }
}
