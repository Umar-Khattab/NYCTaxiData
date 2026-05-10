using FluentValidation;
using MediatR;
using NYCTaxiData.API.Extensions;
using NYCTaxiData.API.Hubs;
using NYCTaxiData.API.Hups.Dispatch;
using NYCTaxiData.API.MiddleWares;
using NYCTaxiData.Application.Behaviors;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Mappings;
using NYCTaxiData.Application.Features.Auth.Commands.Login;
using NYCTaxiData.Infrastructure;
using NYCTaxiData.Infrastructure.Interceptors;
using NYCTaxiData.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Interfaces.Identity;
using NYCTaxiData.Application.Common.Mappings;
using NYCTaxiData.Domain.Common.Interfaces;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Data;
using NYCTaxiData.Infrastructure.Data.Contexts;
using NYCTaxiData.Infrastructure.Data.Repository;
using NYCTaxiData.Infrastructure.Services;
using NYCTaxiData.Infrastructure.Services.Twilio;
using IUnitOfWork = NYCTaxiData.Domain.Interfaces.IUnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. تسجيل الخدمات (قبل الـ Build) =====
// ===== Controllers =====
builder.Services.AddControllers();

builder.Services.AddControllers();
// ===== OpenAPI & Exception Handling =====
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// تسجيل خدمات الـ Infrastructure
builder.Services.AddInfrastructureServices(builder.Configuration);

// ? لازم هنا: تسجيل الـ Authentication و SignalR قبل الـ Build
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(LoginCommandHandler).Assembly);

    // ✅ Pipeline Behaviors
    cfg.AddBehavior(typeof(IPipelineBehavior<,>),
                    typeof(LoggingBehavior<,>));

    cfg.AddBehavior(typeof(IPipelineBehavior<,>),
                    typeof(ValidationBehavior<,>));
});

// ===== Database =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TaxiDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    })
    .EnableSensitiveDataLogging(false)
    .EnableDetailedErrors(false));

// ===== FluentValidation =====
builder.Services.AddValidatorsFromAssembly(
    typeof(LoginCommandHandler).Assembly);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
// ===== AutoMapper =====
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
    cfg.AddProfile<MappingTrips>();
});


// ===== MediatR =====
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(NYCTaxiData.Application.Features.Auth.Commands.Login.LoginCommandHandler).Assembly);
});

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
    cfg.AddProfile<MappingTrips>();
});
// ===== Services =====

builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ISmsService, WhatsAppSmsService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(NYCTaxiData.Application.Features.Auth.Commands.Login.LoginCommandHandler).Assembly);
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<IDispatchNotificationService, DispatchNotification>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(LoginCommandHandler).Assembly);

    cfg.AddBehavior(typeof(IPipelineBehavior<,>),
                    typeof(LoggingBehavior<,>));

    cfg.AddBehavior(typeof(IPipelineBehavior<,>),
                    typeof(ValidationBehavior<,>));

    cfg.AddBehavior(typeof(IPipelineBehavior<,>),
                    typeof(ConcurrencyBehavior<,>)); // ✅ أضف ده
});

// ===== Build & Middleware =====
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseExceptionHandler();

// الترتيب هنا "مقدس": الـ Auth دايماً قبل الـ Hub والـ Controllers
app.UseAuthentication();
app.UseExceptionHandler();
app.UseAuthorization();
app.MapControllers();
app.Run();