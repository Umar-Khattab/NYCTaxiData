using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NYCTaxiData.Application.Common.Interfaces; 
using NYCTaxiData.Application.Common.Interfaces.Services;
using NYCTaxiData.Domain.Common.Interfaces;
using NYCTaxiData.Domain.Interfaces; 
using NYCTaxiData.Infrastructure.Data.Contexts;
using NYCTaxiData.Infrastructure.Data.Repository;
using NYCTaxiData.Infrastructure.Interceptors;
using NYCTaxiData.Infrastructure.Services;

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
            services.AddScoped<ICurrentUserService,CurrentUserService >();
            services.AddScoped<IIdempotencyService, IdempotencyService>(); 
            services.AddScoped<IAiPredictionService, AiPredictionService>();
            services.AddScoped< IDailyAggregationService, DailyAggregationService>();
            services.AddHttpClient<IAiPredictionService, AiPredictionService>(client =>
            {
                client.BaseAddress = new Uri(configuration["AiService:BaseUrl"] ?? "http://localhost:5000");
            }); 
            services.AddScoped<ICacheService, CacheService>();  
            services.AddDistributedMemoryCache();
             
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
            services.AddHttpClient<NYCTaxiData.Application.Common.Interfaces.IAiPredictionService, NYCTaxiData.Infrastructure.Services.AiPredictionService>(client =>
            {
                client.BaseAddress = new Uri(configuration["AiService:BaseUrl"] ?? "http://localhost:5000");
            });

            return services;
        }
    }
}