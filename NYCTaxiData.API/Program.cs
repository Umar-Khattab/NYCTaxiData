using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NYCTaxiData.API.Extensions;
using NYCTaxiData.API.Hubs;
using NYCTaxiData.API.Hups.Dispatch;
using NYCTaxiData.API.Hups.Simulation;
using NYCTaxiData.API.MiddleWares;
using NYCTaxiData.Application;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Interfaces.Services;
using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Application.Common.Mappings;
using NYCTaxiData.Infrastructure;
using NYCTaxiData.Infrastructure.Data.Contexts;
using NYCTaxiData.Infrastructure.Services;
using NYCTaxiData.Infrastructure.Services.Twilio;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ Application Layer
builder.Services.AddApplicationServices(builder.Configuration);

// ✅ Infrastructure Layer
builder.Services.AddInfrastructureServices(builder.Configuration);

// ✅ Health Checks
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
var aiConn = builder.Configuration.GetConnectionString("AiConnection") ?? string.Empty;
var mlUrl  = builder.Configuration["MlService:BaseUrl"] ?? "http://127.0.0.1:8000/";
builder.Services.AddHealthChecks()
    .AddNpgSql(defaultConn, name: "postgres-main",  tags: new[] { "ready", "db" })
    .AddNpgSql(aiConn,      name: "postgres-ai",    tags: new[] { "ready", "db" })
    .AddUrlGroup(new Uri(mlUrl.TrimEnd('/') + "/health"), name: "ml-service", tags: new[] { "ready" });

// ✅ API Services
builder.Services.AddControllers();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddMvc();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();  
builder.Services.AddScoped<ISmsService, WhatsAppSmsService>();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("SimulationCors", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                      new[] { "http://localhost:5173" };
        policy.WithOrigins(origins)
            .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "X-Idempotency-Key")
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
            .AllowCredentials();
    });
});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<IDispatchNotificationService, DispatchNotification>();
builder.Services.AddSingleton<ISimulationEventStreamer, SimulationEventStreamer>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AIMappingProfile>();
    cfg.AddProfile<MappingProfile>();
});
builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("SimulationCors");
app.MapControllers();

// ✅ Health Check Endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = hc => hc.Tags.Contains("ready")
});

// ✅ SignalR Hubs
app.MapHub<TaxiHub>("/hubs/taxi");
app.MapHub<LiveTrackingHub>("/hubs/tracking");
app.MapHub<DispatchHub>("/hubs/dispatch");
app.MapHub<SimulationHub>("/hubs/simulation"); 

app.Run();