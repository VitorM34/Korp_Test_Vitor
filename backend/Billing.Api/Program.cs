using Billing.Api;
using Billing.Api.Application.Services;
using Billing.Api.Infrastructure;
using Billing.Api.Infrastructure.Http;
using Billing.Api.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Billing API", Version = "v1" }));

builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BillingDb")));

var inventoryBaseUrl = builder.Configuration["InventoryApi:BaseUrl"]
    ?? throw new InvalidOperationException("InventoryApi:BaseUrl não configurado.");

builder.Services.AddHttpClient<IInventoryApiClient, InventoryApiClient>(client =>
{
    client.BaseAddress = new Uri(inventoryBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.Delay = TimeSpan.FromMilliseconds(500);
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.MinimumThroughput = 3;
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<InvoiceService>();
builder.Services.AddHostedService<WarmupService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Billing API v1"));

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    db.Database.Migrate();
}

app.Run();
