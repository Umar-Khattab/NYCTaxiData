using NYCTaxiData.Infrastructure; // „Â„ Ãœ« ⁄‘«‰ Ì‘Ê› AddInfrastructureServices
using NYCTaxiData.Application.Common.Mappings;
using NYCTaxiData.API.MiddleWares;

var builder = WebApplication.CreateBuilder(args);

// 1. ===== Controllers & Essentials =====
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // ·Ê » ” Œœ„ .NET 9/10 «·ÃœÌœ
builder.Services.AddProblemDetails();

// 2. ===== Infrastructure Layer (Database, Repositories, UnitOfWork) ===== 
builder.Services.AddInfrastructureServices(builder.Configuration);

// 3. ===== AutoMapper ===== 
builder.Services.AddAutoMapper(cfg => 
{ 
    cfg.AddProfile<MappingProfile>();

    cfg.AddProfile<MappingTrips>(); 
});

// 4. ===== MediatR ===== 
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(NYCTaxiData.Application.Features.Auth.Commands.Login.LoginCommandHandler).Assembly);
});

// 5. ===== Global Exception Handling =====
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// 6. ===== Middlewares Pipeline =====

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // ·Ê » ” Œœ„ SwaggerUI ÷Ì›Â Â‰«
}

app.UseHttpsRedirection();

//  ›⁄Ì· «·‹ Exception Handling Middleware
app.UseExceptionHandler();

//  ›⁄Ì· «·‹ Routing Ê«·‹ Auth
app.UseAuthorization();

app.MapControllers();

app.Run();