using Microsoft.EntityFrameworkCore;
using Npgsql;
using NYCTaxiData.Application.Common.Interfaces.Identity;
using NYCTaxiData.Domain.Common.Interfaces;
using NYCTaxiData.Domain.DTOs.Identity;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Data;
using NYCTaxiData.Infrastructure.Data.Contexts;
using NYCTaxiData.Infrastructure.Data.Repository;
using NYCTaxiData.Infrastructure.Services;
using NYCTaxiData.Infrastructure.Services.Twilio;
using System.Reflection;
using NYCTaxiData.Application.Common.Mappings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// OpenAPI & Exception Handling
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<NYCTaxiData.API.MiddleWares.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TaxiDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    }));

// --- نهاية الجزء المصحح ---

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<ISmsService, WhatsAppSmsService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// تأكد إنك عامل using AutoMapper; فوق
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(NYCTaxiData.Application.Features.Auth.Commands.Login.LoginCommandHandler).Assembly);
});
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
} 

app.UseExceptionHandler(); 
app.UseAuthorization();

// -----------------------
// 8️⃣ Map Controllers
// -----------------------
app.MapControllers();


app.Run();