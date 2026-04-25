// Path: NYCTaxiData.Infrastructure/DependencyInjection.cs
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
using Microsoft.AspNetCore.Http;
using MediatR;

// ... (باقي الـ usings)

namespace NYCTaxiData.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // 1. تسجيل الخدمات المساعدة أولاً
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<AuditLogInterceptor>();

            // 2. تسجيل الـ DbContext مرة واحدة فقط
            services.AddDbContext<TaxiDbContext>((sp, options) =>
            {
                // سحب الـ Interceptors من الـ Container داخل الـ scope الحالي
                var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
                var auditLogInterceptor = sp.GetRequiredService<AuditLogInterceptor>();

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                })
                .AddInterceptors(auditableInterceptor, auditLogInterceptor)  
                .EnableSensitiveDataLogging(false)
                .EnableDetailedErrors(false);
            });
             
            services.AddScoped<JwtTokenService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddScoped<ISmsService, WhatsAppSmsService>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
          

            return services;
        }
    }
}
