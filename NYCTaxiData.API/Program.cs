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

var builder = WebApplication.CreateBuilder(args);

// ===== 1. تسجيل الخدمات (قبل الـ Build) =====

builder.Services.AddControllers();
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

// ===== FluentValidation =====
builder.Services.AddValidatorsFromAssembly(
    typeof(LoginCommandHandler).Assembly);

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
    cfg.AddProfile<MappingTrips>();
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(NYCTaxiData.Application.Features.Auth.Commands.Login.LoginCommandHandler).Assembly);
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<IDispatchNotificationService, DispatchNotification>();
builder.Services.AddScoped<JwtTokenService>();
var app = builder.Build();

// ===== 2. إعداد الـ Middleware Pipeline (بعد الـ Build) =====

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

// الترتيب هنا "مقدس": الـ Auth دايماً قبل الـ Hub والـ Controllers
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<TaxiHub>("/hubs/taxi");
app.MapHub<LiveTrackingHub>("/hubs/tracking");
app.MapControllers();
app.MapHub<DispatchHub>("/hubs/dispatch");


app.Run();