using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParallelWorld.Application.Messaging;

namespace ParallelWorld.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddAiTextGeneration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<AiGenerationOptions>()
            .Bind(configuration.GetSection(AiGenerationOptions.SectionName))
            .Validate(
                AiGenerationOptions.IsValid,
                "AI generation configuration must provide a valid server-controlled Ollama endpoint, "
                    + "model, timeout, output limit, and prompt-template version.")
            .ValidateOnStart();
        services.AddHttpClient<OllamaTextProvider>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<AiGenerationOptions>>()
                .Value;
            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            client.Timeout = Timeout.InfiniteTimeSpan;
        }).ConfigurePrimaryHttpMessageHandler(OllamaHttpHandlerFactory.Create);
        services.AddScoped<IAiTextProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<OllamaTextProvider>());
        services.AddSingleton<IAiPromptBuilder, AiPromptBuilder>();
        services.AddSingleton<IAiFallbackRenderer, DeterministicAiFallbackRenderer>();
        services.AddSingleton<IAiOutputValidator, AiOutputValidator>();
        services.AddSingleton<IAiCanonicalInputFactory, AiCanonicalInputFactory>();
        services.AddScoped<IAiTextGenerator, AiTextGenerator>();
        services.AddScoped<IMessageWordingGenerator, MessageWordingGenerator>();
        services.AddHostedService<MessageWordingWorker>();
        return services;
    }
}
