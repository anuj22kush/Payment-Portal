using Microsoft.OpenApi.Models;
using Payments_Portal.Data;
using Payments_Portal.Service;

var builder = WebApplication.CreateBuilder(args);

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payments Portal API",
        Version = "v1",
        Description = "API documentation for Payments Portal"
    });
});

// Add services to the container.

builder.Services.AddControllers();

// Storage provider: "Json" for file-based persistence, "InMemory" for in-memory storage
var storageProvider = builder.Configuration.GetValue<string>("StorageProvider") ?? "Json";

if (storageProvider.Equals("Json", StringComparison.OrdinalIgnoreCase))
{
    var jsonFilePath = Path.Combine(builder.Environment.ContentRootPath, "Data", "payments.json");
    builder.Services.AddSingleton<IPaymentRepository>(new JsonPaymentRepository(jsonFilePath));
}
else
{
    builder.Services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();
}

// Service-layer DI — each abstraction maps to a single implementation (DIP/SRP)
builder.Services.AddSingleton<IReferenceGenerator, DailySequentialReferenceGenerator>();
builder.Services.AddSingleton<IPaymentMapper, PaymentMapper>();
builder.Services.AddSingleton<IPaymentValidator, PaymentValidator>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments Portal API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at application root
    });
}

app.UseCors("AllowAngular");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
