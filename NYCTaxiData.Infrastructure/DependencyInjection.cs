using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using NYCTaxiData.Application.Common.Interfaces.Services;
using NYCTaxiData.Domain.Common.Interfaces;
using NYCTaxiData.Infrastructure.Data;
using NYCTaxiData.Infrastructure.Data.Contexts;
using NYCTaxiData.Infrastructure.Data.Repository;
using NYCTaxiData.Infrastructure.Interceptors;
using NYCTaxiData.Infrastructure.Services;
using NYCTaxiData.Infrastructure.Workers;

namespace NYCTaxiData.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
             
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<AuditLogInterceptor>(); 
             
            services.AddScoped<ICacheService, CacheService>(); 
            services.AddScoped<ISmsService, WhatsAppSmsService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddDistributedMemoryCache();
            // ===== Services =====
            services.AddScoped<IDailyAggregationService, DailyAggregationService>();

            // ===== Background Worker =====
            services.AddHostedService<DailyAggregationWorker>();
             
            services.AddDbContext<TaxiDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(60); // increase seconds
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 2,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                }));
             
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAiPredictionService, AiPredictionService>();
            services.AddHttpClient("MlService", client =>
            {
                client.BaseAddress = new Uri(configuration["MlService:BaseUrl"]!);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

            
            return services;
        }
    }
}