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
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<AuditLogInterceptor>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IIdempotencyService, IdempotencyService>();
            services.AddScoped<IAiPredictionService, AiPredictionService>();
            services.AddScoped<IDailyAggregationService, DailyAggregationService>();
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
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        })
        // Use a local DelegatingHandler to apply the Polly policy without requiring PolicyHttpMessageHandler type
        .AddHttpMessageHandler(sp =>
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            return new PolicyDelegatingHandler(retryPolicy);
        });
            services.AddScoped<ICacheService, CacheService>();
            services.AddDistributedMemoryCache();
            //services.AddDbContext<AiDbContext>(options =>
            //options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection2")));

            services.AddDbContext<TaxiDbContext>((sp, options) =>
            {
                var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
                var auditLogInterceptor = sp.GetRequiredService<AuditLogInterceptor>();

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                })
                .AddInterceptors(auditableInterceptor, auditLogInterceptor);
            });
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
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