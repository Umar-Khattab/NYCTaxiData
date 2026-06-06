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
    .AddAsyncCheck("postgres-main", async () =>
    {
        try
        {
            using var conn = new NpgsqlConnection(defaultConn);
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1;";
            cmd.CommandTimeout = 3;
            await cmd.ExecuteScalarAsync();
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Postgres Main is down", ex);
        }
    }, tags: new[] { "ready", "db" })
    .AddAsyncCheck("postgres-ai", async () =>
    {
        try
        {
            using var conn = new NpgsqlConnection(aiConn);
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1;";
            cmd.CommandTimeout = 3;
            await cmd.ExecuteScalarAsync();
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            // Graceful degradation: AI DB is non-critical, so we report healthy but degraded
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Postgres AI is degraded or paused: " + ex.Message);
        }
    }, tags: new[] { "ready", "db" })
    .AddUrlGroup(new Uri(mlUrl.TrimEnd('/') + "/openapi.json"), name: "ml-service", tags: new[] { "ready" });

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
                      new[] { "http://localhost:5173","http://localhost:3000","https://nyc-taxi-front.vercel.app", "https://nyc-taxi-front.vercel.app/" };
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

// ✅ Database Index Optimization Startup Check
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var config = services.GetRequiredService<IConfiguration>();
    var defaultConnStr = config.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(defaultConnStr))
    {
        try
        {
            logger.LogInformation("Verifying optimized database indexes...");
            using var conn = new NpgsqlConnection(defaultConnStr);
            await conn.OpenAsync();
            
            using var cmd = conn.CreateCommand();
            cmd.CommandTimeout = 300; // Allow sufficient time for index creation
            
            logger.LogInformation("Creating index idx_trips_started_pickup_fare if not exists...");
            cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_trips_started_pickup_fare ON trips (started_at DESC, pickup_location_id) INCLUDE (fare_amount);";
            await cmd.ExecuteNonQueryAsync();

            logger.LogInformation("Creating index idx_trips_driver_started_dropoff if not exists...");
            cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_trips_driver_started_dropoff ON trips (driver_id, started_at DESC, dropoff_location_id);";
            await cmd.ExecuteNonQueryAsync();

            logger.LogInformation("Creating index idx_trips_status_pickup if not exists...");
            cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_trips_status_pickup ON trips (process_status, pickup_location_id) WHERE process_status = 'Ongoing';";
            await cmd.ExecuteNonQueryAsync();

            logger.LogInformation("Creating index idx_drivers_status if not exists...");
            cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_drivers_status ON drivers (status);";
            await cmd.ExecuteNonQueryAsync();

            logger.LogInformation("Creating index idx_location_zone_id if not exists...");
            cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_location_zone_id ON location (zone_id) WHERE zone_id IS NOT NULL;";
            await cmd.ExecuteNonQueryAsync();

            logger.LogInformation("Database indexes verified/created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating database indexes.");
        }
    }
}

app.Run();