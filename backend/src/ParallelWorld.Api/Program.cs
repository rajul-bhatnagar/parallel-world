using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ParallelWorld.Api.Endpoints;
using ParallelWorld.Api.Errors;
using ParallelWorld.Api.Health;
using ParallelWorld.Api.Observability;
using ParallelWorld.Application.Simulation;
using ParallelWorld.Infrastructure;
using ParallelWorld.Simulation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.With<SensitiveDataEnricher>()
        .WriteTo.Console());

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(ProblemDetailsConfiguration.Configure);
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IDeterministicRandomProvider, DeterministicRandomProvider>();
builder.Services.AddSingleton<IWorldTimeProjector, WorldTimeProjector>();
builder.Services.AddSingleton<IDeterministicTemplateGenerator, DeterministicTemplateGenerator>();
builder.Services.AddSingleton<ISimulationRule, ActSimulationRule>();
builder.Services.AddSingleton<ISimulationRule, PostSimulationRule>();
builder.Services.AddSingleton<ISimulationRule, ReplySimulationRule>();
builder.Services.AddSingleton<ISimulationRule, ReactSimulationRule>();
builder.Services.AddSingleton<ISimulationRule, FollowSimulationRule>();
builder.Services.AddScoped<ISimulationService, SimulationService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("development-simulation", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            EndpointResults.GetUserId(context.User)?.ToString("N") ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1),
            }));
});
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck<PostgreSqlHealthCheck>(PostgreSqlHealthCheck.Name, tags: ["ready"]);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
});
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapDevelopmentSimulationEndpoints();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live"),
    ResponseWriter = HealthResponseWriter.WriteAsync,
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = HealthResponseWriter.WriteAsync,
});

if (app.Environment.IsEnvironment("Testing"))
{
    app.MapGet("/_testing/error", ThrowTestException);
}

app.MapAuthenticationEndpoints();
app.MapWorldEndpoints();
app.MapCharacterEndpoints();
app.MapSocialFeedEndpoints();

await app.RunAsync();

static IResult ThrowTestException() =>
    throw new InvalidOperationException(
        "Password=private-test-password;RefreshToken=private-test-refresh-token");

public partial class Program;
