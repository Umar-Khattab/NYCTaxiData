using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NYCTaxiData.Application.Common.Interfaces.Identity;
using NYCTaxiData.Domain.Common.Interfaces;
using NYCTaxiData.Domain.Interfaces;
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

            
            return services;
        }
    }
}