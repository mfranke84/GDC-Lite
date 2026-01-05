using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

// App Bootstrapping
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Ready flag state
builder.Services.AddSingleton<ReadyState>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Variables
var appIsReady = false;


// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddCheck<ReadyHealthCheck>("ready");

var app = builder.Build();

// Set ready=true when app started
var readyState = app.Services.GetRequiredService<ReadyState>();
app.Lifetime.ApplicationStarted.Register(() => readyState.IsReady = true);
app.Lifetime.ApplicationStarted.Register(() => appIsReady = true);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.MapControllers();

// Endpoints
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = r => r.Name == "self" });
app.MapHealthChecks("/ready",  new HealthCheckOptions { Predicate = r => r.Name == "ready" });


app.Run();

// ---- Helpers ----
public sealed class ReadyState
{
    public bool IsReady { get; set; } = false;
}

public sealed class ReadyHealthCheck : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
{
    private readonly ReadyState _state;
    private readonly IConfiguration _configuration;

    public ReadyHealthCheck(ReadyState state, IConfiguration configuration)
    {
        _state = state;
        _configuration = configuration;
    }

    public Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Simulated dependency: "DatabaseAvailable"
        var dbOk = _configuration.GetValue("Dependencies:DatabaseAvailable", true);

        if (!_state.IsReady)
            return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Starting up"));

        if (!dbOk)
            return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Database not available"));

        return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Ready"));
    }
}


