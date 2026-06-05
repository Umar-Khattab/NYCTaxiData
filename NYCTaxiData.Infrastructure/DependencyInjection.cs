using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Interfaces.Services;
using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Domain.Common.Interfaces;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Data.Contexts;
using NYCTaxiData.Infrastructure.Data.Repository;
using NYCTaxiData.Infrastructure.Interceptors;
using NYCTaxiData.Infrastructure.Services;
using NYCTaxiData.Infrastructure.Simulation;
using Polly;
using Polly.Extensions.Http;

namespace NYCTaxiData.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. قراءة الـ Connection Strings
            var defaultConn = configuration.GetConnectionString("DefaultConnection");
            var aiConn = configuration.GetConnectionString("AiConnection");

            if (string.IsNullOrEmpty(defaultConn) || string.IsNullOrEmpty(aiConn))
            {
                throw new InvalidOperationException("Connection strings are missing in appsettings.json!");
            }

            // 2. تسجيل الـ DbContexts
            services.AddDbContext<AiDbContext>(options =>
                options.UseNpgsql(aiConn, npgsql => {
                    npgsql.CommandTimeout(45); // Safe threshold under MediatR PerformanceBehavior
                    npgsql.EnableRetryOnFailure(3);
                }));

            services.AddDbContext<TaxiDbContext>((sp, options) =>
            {
                var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
                var auditLogInterceptor = sp.GetRequiredService<AuditLogInterceptor>();
                options.UseNpgsql(defaultConn, npgsql => {
                    npgsql.CommandTimeout(45);
                    npgsql.EnableRetryOnFailure(5);
                })
                       .AddInterceptors(auditableInterceptor, auditLogInterceptor);
            });

            // 3. تسجيل الخدمات الأساسية (التي كانت تسبب الأخطاء)
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IIdempotencyService, IdempotencyService>();

            // 4. خدمات الـ AI والـ Simulation
            services.AddTransient<HostFailoverDelegatingHandler>();
            services.AddScoped<IAiTemporalResolver, AiTemporalResolver>();
            services.AddScoped<IAiPredictionService, AiPredictionService>();
            services.AddScoped<IDailyAggregationService, DailyAggregationService>();
            services.AddScoped<AiFeatureProvider>();
            services.AddScoped<IAiFeatureProvider>(sp =>
                new CachingAiFeatureProvider(
                    sp.GetRequiredService<AiFeatureProvider>(),
                    sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
                    sp.GetRequiredService<IAiTemporalResolver>()));

            services.Configure<SimulationOptions>(configuration.GetSection("Simulation"));
            services.AddSingleton<ISimulationFeatureLoader, SimulationFeatureLoader>();
            services.AddSingleton<ISimulationStateManager, SimulationStateManager>();
            services.AddSingleton<ISimulationRuleEngine, SimulationRuleEngine>();
            services.AddSingleton<ISimulationResultStore, SimulationResultStore>();
            services.AddSingleton<ISimulationOrchestrator, SimulationOrchestrator>();
            // C#
            services
        .AddHttpClient<IAiPredictionService, AiPredictionService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var baseUrl = config["MlService:BaseUrl"] ?? "http://127.0.0.1:8000/";
            client.BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
        })
        .AddHttpMessageHandler<HostFailoverDelegatingHandler>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        })
        // Use a local DelegatingHandler to apply the Polly policy without requiring PolicyHttpMessageHandler type
        // Polly retry removed: RetryBehavior in the MediatR pipeline already handles
        // transient failures at the handler level. Having both would cause up to 9 ML
        // service calls per request (3 MediatR retries × 3 HTTP retries).
        ;

            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<AuditLogInterceptor>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            return services;
        }

        // Local DelegatingHandler to run an IAsyncPolicy<HttpResponseMessage> without depending on external PolicyHttpMessageHandler type
        private class PolicyDelegatingHandler : DelegatingHandler
        {
            private readonly IAsyncPolicy<HttpResponseMessage> _policy;

            public PolicyDelegatingHandler(IAsyncPolicy<HttpResponseMessage> policy)
            {
                _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Execute the HTTP call within the Polly policy
                return _policy.ExecuteAsync(ct => base.SendAsync(request, ct), cancellationToken);
            }
        }
    }
}