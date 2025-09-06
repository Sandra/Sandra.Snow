using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;
using Snow.Engine.IO;
using Snow.Engine.Markdown;
using Snow.Engine.Parsing;
using Snow.Engine.Pipeline;
using Snow.Engine.Processing;
using Snow.Engine.Templates;

namespace Snow.Engine.Extensions;

/// <summary>
/// Extension methods for configuring Sandra.Snow services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Sandra.Snow core services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSnowCore(this IServiceCollection services)
    {
        // Core services
        services.AddSingleton<IFileProvider, FileSystemProvider>();
        services.AddSingleton<IMarkdownParser>(provider =>
        {
            var config = MarkdownConfiguration.Default();
            var logger = provider.GetRequiredService<ILogger<MarkdigMarkdownParser>>();
            return new MarkdigMarkdownParser(config, logger);
        });

        // Parsers
        services.AddSingleton<IPostParser, PostParser>();
        services.AddSingleton<IPageParser, PageParser>();

        // Template system
        services.AddSingleton<ITemplateRenderer>(provider =>
        {
            var fileProvider = provider.GetRequiredService<IFileProvider>();
            var settings = provider.GetRequiredService<ISiteSettings>();
            var logger = provider.GetRequiredService<ILogger<SimpleTemplateRenderer>>();
            return new SimpleTemplateRenderer(fileProvider, settings, logger);
        });

        // Pipeline components
        services.AddSingleton<IContentReader, ContentReader>();
        services.AddSingleton<IPageWriter, PageWriter>();
        services.AddSingleton<SiteGenerator>();

        // Content processors
        services.AddSingleton<ProcessorFactory>();
        services.AddTransient<IContentProcessor, Processing.Processors.PostsProcessor>();

        return services;
    }

    /// <summary>
    /// Adds the Sandra.Snow generation pipeline.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSnowPipeline(this IServiceCollection services)
    {
        // The SiteGenerator handles the entire pipeline internally
        // Additional pipeline components can be registered here as needed
        
        return services;
    }

    /// <summary>
    /// Adds logging configuration optimized for static site generation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="enableDebugLogging">Whether to enable debug-level logging.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSnowLogging(this IServiceCollection services, bool enableDebugLogging = false)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();

            if (enableDebugLogging)
            {
                builder.SetMinimumLevel(LogLevel.Debug);
            }
            else
            {
                builder.SetMinimumLevel(LogLevel.Information);
            }

            // Filter noisy framework logs
            builder.AddFilter("Microsoft", LogLevel.Warning);
            builder.AddFilter("System", LogLevel.Warning);
        });

        return services;
    }

    /// <summary>
    /// Adds configuration for Sandra.Snow with validation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settings">The site settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSnowConfiguration(this IServiceCollection services, ISiteSettings settings)
    {
        services.AddSingleton(settings);
        
        // TODO: Add configuration validation here
        // services.AddOptions<SiteSettings>()
        //         .Bind(configuration.GetSection("Snow"))
        //         .ValidateDataAnnotations();

        return services;
    }

    /// <summary>
    /// Adds all Sandra.Snow services with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settings">The site settings.</param>
    /// <param name="enableDebugLogging">Whether to enable debug logging.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSnow(
        this IServiceCollection services, 
        ISiteSettings settings, 
        bool enableDebugLogging = false)
    {
        return services
            .AddSnowCore()
            .AddSnowPipeline() 
            .AddSnowConfiguration(settings)
            .AddSnowLogging(enableDebugLogging);
    }
}
