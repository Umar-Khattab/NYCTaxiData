using NYCTaxiData.API.MiddleWares;
using NYCTaxiData.Application; // للوصول لـ AddApplicationServices
using NYCTaxiData.Infrastructure; // للوصول لـ AddInfrastructureServices (ستقوم بإنشائه)

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers & API Docs
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 2. Exception Handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 3. Layer Registrations (Here is the Clean Architecture Magic ✨)
// بدلاً من كتابة 50 سطراً هنا، نستدعي الطبقات لتعرف نفسها
builder.Services.AddApplicationServices(); // هذا سيقوم بتسجيل MediatR والـ Behaviors والـ AutoMapper
builder.Services.AddInfrastructureServices(builder.Configuration); // هذا سيسجل الـ DbContext والـ Repositories

var app = builder.Build();

// 4. Run Database Initializer (Seeding) safely on startup
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    dbInitializer.Initialize(); // سيقوم بعمل Migrate و Seed
}

// 5. HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Note: You might want to add Scalar or SwaggerUI here for visual testing
}

app.UseExceptionHandler(); 

// 6. Security Pipeline (ORDER IS CRITICAL)
app.UseAuthentication(); // 👈 يجب أن يكون قبل Authorization!
app.UseAuthorization();

// 7. Map Endpoints
app.MapControllers();

app.Run();
