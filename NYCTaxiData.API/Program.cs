using NYCTaxiData.API.Extensions;
using NYCTaxiData.API.Hubs;
using NYCTaxiData.API.Hups.Dispatch; 
using NYCTaxiData.API.Hups.Simulation;
using NYCTaxiData.API.MiddleWares;
using NYCTaxiData.Application;
using NYCTaxiData.Application.Common.Interfaces;  
using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Application.Common.Interfaces.Services;
using NYCTaxiData.Application.Common.Mappings;
using NYCTaxiData.Infrastructure;
using NYCTaxiData.Infrastructure.Services;
using NYCTaxiData.Infrastructure.Services.Twilio;

var builder = WebApplication.CreateBuilder(args);

// ✅ Application Layer
builder.Services.AddApplicationServices(builder.Configuration);

// ✅ Infrastructure Layer
builder.Services.AddInfrastructureServices(builder.Configuration);

// ✅ API Services
builder.Services.AddControllers();
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
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<IDispatchNotificationService, DispatchNotification>();
builder.Services.AddSingleton<ISimulationEventStreamer, SimulationEventStreamer>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AIMappingProfile>();
    cfg.AddProfile<MappingProfile>();
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("SimulationCors");
app.MapControllers();

// ✅ SignalR Hubs
app.MapHub<TaxiHub>("/hubs/taxi");
app.MapHub<LiveTrackingHub>("/hubs/tracking");
app.MapHub<DispatchHub>("/hubs/dispatch");
app.MapHub<SimulationHub>("/hubs/simulation");

app.Run();