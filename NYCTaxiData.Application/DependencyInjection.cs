using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NYCTaxiData.Application.Behaviors;
using System.Reflection;

namespace NYCTaxiData.Application
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers all Application layer services:
        /// - FluentValidation validators
        /// - MediatR (handlers + pipeline behaviors)
        /// - AutoMapper (profiles + license)
        /// </summary>
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // =========================================================
            // 1. FluentValidation
            // Automatically scans and registers all validators
            // =========================================================
            services.AddValidatorsFromAssembly(assembly);

            // =========================================================
            // 2. MediatR Registration
            // Handles CQRS (Commands / Queries / Handlers)
            // =========================================================
            services.AddMediatR(config =>
            {
                // Register all handlers from current assembly
                config.RegisterServicesFromAssembly(assembly);

                // ✅ License Key (recommended to load from configuration, not hardcoded)
                config.LicenseKey = configuration["MediatR:LicenseKey"];

                // =======================================================================
                // PIPELINE BEHAVIORS (Execution order: Outermost → Innermost)
                // =======================================================================

                // 1. Global Exception Handling
                // Catches exceptions from ALL inner behaviors and handlers
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

                // 2. Observability (Monitoring & Logging)
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

                // 3. Authorization (Security Gate)
                // Ensures user is allowed BEFORE doing any heavy work
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

                // 4. Validation
                // Validates request data after authorization
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

                // 5. Caching
                // Returns cached responses if available (only for valid + authorized requests)
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

                // 6. Idempotency
                // Prevents duplicate processing (important for commands with side effects)
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));

                // 7. Resilience (Retry + Timeout)
                // Wraps execution with retry and timeout policies
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TimeoutBehavior<,>));

                // 8. Transaction (Innermost)
                // Ensures DB operations are atomic (last step before handler execution)
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            });

            // =========================================================
            // 3. AutoMapper Registration
            // Handles mapping between Entities ↔ DTOs ↔ Commands
            // =========================================================
            services.AddAutoMapper(cfg =>
            {
                // ✅ License Key
                cfg.LicenseKey = configuration["AutoMapper:LicenseKey"];

                // You can also configure global mapping options here if needed
            }, assembly);

            return services;
        }
    }
}
