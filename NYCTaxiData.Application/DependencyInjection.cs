using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NYCTaxiData.Application.Behaviors;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Domain.Interfaces;
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

                // Pipeline execution order: first registered = outermost wrapper.
                // ExceptionHandling → Metrics → Performance → Logging → Authorization
                // → Validation → Idempotency → Caching → Timeout → Retry → Transaction

                // 1. Outermost: catches all exceptions from inner behaviors
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

                // 2. Observability
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

                // 3. Security gate — validate identity before doing any work
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

                // 4. Input validation — reject bad requests early
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

                // 5. Idempotency — deduplicate before hitting cache or DB
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));

                // 6. Caching — return early if response is cached
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

                // 7. Timeout — each individual attempt gets its own budget
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TimeoutBehavior<,>));

                // 8. Retry — retries the full Timeout+Transaction block on transient failure
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));

                // 9. Innermost: wraps DB writes in a transaction
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