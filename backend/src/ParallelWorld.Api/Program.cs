using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ParallelWorld.AI;
using ParallelWorld.Api;
using ParallelWorld.Api.Configuration;
using ParallelWorld.Api.Endpoints;
using ParallelWorld.Api.Errors;
using ParallelWorld.Api.Health;
using ParallelWorld.Api.Observability;
using ParallelWorld.Application.Simulation;
using ParallelWorld.Infrastructure;
using ParallelWorld.Simulation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = builder.Configuration.GetValue(
        "Deployment:MaxRequestBodyBytes",
        DeploymentOptions.DefaultMaxRequestBodyBytes));

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
var hardenedEnvironment = builder.Environment.IsStaging() || builder.Environment.IsProduction();
builder.Services
    .AddOptions<DeploymentOptions>()
    .Bind(builder.Configuration.GetSection(DeploymentOptions.SectionName))
    .Validate(options => DeploymentOptions.IsValid(options, hardenedEnvironment),
        "Deployment configuration must provide bounded request limits and explicit production Host/CORS allowlists.")
    .ValidateOnStart();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddAiTextGeneration(builder.Configuration);
builder.Services.AddCors(options => options.AddPolicy("configured-origins", policy =>
{
    var origins = builder.Configuration.GetSection("Deployment:CorsAllowedOrigins").Get<string[]>() ?? [];
    if (origins.Length > 0)
    {
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    }
}));
builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(
    ProductionForwarding.Configure);
builder.Services.AddSingleton<IDeterministicRandomProvider, DeterministicRandomProvider>();
builder.Services.AddSingleton<IWorldTimeProjector, WorldTimeProjector>();
builder.Services.AddSingleton<IDeterministicTemplateGenerator, DeterministicTemplateGenerator>();
builder.Services.AddSingleton<ISimulationRule, ActSimulationRule>();
builder.Services.AddSingleton<ISimulationRule, PostSimulationRule>();
builder.Services.AddSingleton<ISimulationRule, ReplySimulationRule>();
builder.Services.AddSingleton<ISimulationRule, ReactSimulationRule>();
builder.Services.AddSingleton<ISimulationRule, FollowSimulationRule>();
builder.Services.AddScoped<ISimulationService, SimulationService>();
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<CatchUpRecoveryWorker>();
}
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
    options.AddPolicy("catch-up", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            EndpointResults.GetUserId(context.User)?.ToString("N") ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 4,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1),
            }));
});
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck<PostgreSqlHealthCheck>(PostgreSqlHealthCheck.Name, tags: ["ready"]);

var app = builder.Build();

if (hardenedEnvironment)
{
    app.UseForwardedHeaders();
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    var deployment = context.RequestServices.GetRequiredService<IOptions<DeploymentOptions>>().Value;
    if (hardenedEnvironment
        && !deployment.AllowedHosts.Contains(context.Request.Host.Host, StringComparer.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    if (context.Request.ContentLength > deployment.MaxRequestBodyBytes)
    {
        context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
        return;
    }

    context.Response.OnStarting(() =>
    {
        context.Response.Headers.XContentTypeOptions = "nosniff";
        context.Response.Headers.XFrameOptions = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        return Task.CompletedTask;
    });
    await next();
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
});
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors("configured-origins");
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
app.MapRelationshipEndpoints();
app.MapMessagingEndpoints();
app.MapCatchUpEndpoints();
app.MapNotificationEndpoints();

await app.RunAsync();

static IResult ThrowTestException() =>
    throw new InvalidOperationException(
        "Password=private-test-password;RefreshToken=private-test-refresh-token");

public partial class Program;
