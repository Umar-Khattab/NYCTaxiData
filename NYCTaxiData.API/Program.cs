using NYCTaxiData.API.MiddleWares;
using NYCTaxiData.Application; 
using NYCTaxiData.Infrastructure;
using NYCTaxiData.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. Controllers & API Documentation
// =========================================================
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // يمكنك لاحقاً إضافة Swagger UI أو Scalar

// =========================================================
// 2. Global Exception Handling
// =========================================================
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// =========================================================
// 3. Register Application & Infrastructure Layers
// (Clean Architecture Entry Point ✨)
// =========================================================

// ✅ مهم: تمرير Configuration عشان الـ license keys تشتغل
builder.Services.AddApplicationServices(builder.Configuration);

// Infrastructure (DbContext, Repositories, External Services)
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// =========================================================
// 4. Database Initialization (Migrate + Seed)
// يتم تشغيله مرة واحدة عند startup
// =========================================================
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await dbInitializer.InitializeAsync();
}

// =========================================================
// 5. HTTP Request Pipeline
// =========================================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Global exception handler middleware
app.UseExceptionHandler();

// =========================================================
// 6. Security Pipeline (ORDER IS CRITICAL)
// =========================================================
app.UseAuthentication(); // لازم قبل Authorization
app.UseAuthorization();

// =========================================================
// 7. Endpoints Mapping
// =========================================================
app.MapControllers();

app.Run();
