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

namespace NYCTaxiData.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // 1. تسجيل الخدمات المساعدة (Infrastructure Helpers)
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<AuditLogInterceptor>();

            // خدمات إضافية (شغل صاحبك)
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddScoped<ISmsService, WhatsAppSmsService>();
            services.AddDistributedMemoryCache();

            // 2. تسجيل الـ DbContext مع الـ Interceptors والـ Retry Logic
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

            // 3. تسجيل أنماط البيانات (Data Patterns)
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}