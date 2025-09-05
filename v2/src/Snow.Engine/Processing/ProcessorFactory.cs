using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;

namespace Snow.Engine.Processing;

/// <summary>
/// Factory for discovering and creating content processors.
/// </summary>
public class ProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessorFactory> _logger;
    private readonly Dictionary<string, Type> _processorTypes;

    public ProcessorFactory(IServiceProvider serviceProvider, ILogger<ProcessorFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _processorTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        
        DiscoverProcessors();
    }

    /// <summary>
    /// Gets a processor by name.
    /// </summary>
    /// <param name="processorName">The processor name.</param>
    /// <returns>The processor instance or null if not found.</returns>
    public IContentProcessor? GetProcessor(string processorName)
    {
        if (string.IsNullOrWhiteSpace(processorName))
            return null;

        if (_processorTypes.TryGetValue(processorName, out var processorType))
        {
            try
            {
                return (IContentProcessor?)Activator.CreateInstance(processorType, _serviceProvider.GetService(typeof(ILogger<>).MakeGenericType(processorType)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create processor {ProcessorName}", processorName);
                return null;
            }
        }

        _logger.LogWarning("Processor {ProcessorName} not found", processorName);
        return null;
    }

    /// <summary>
    /// Gets all available processor names.
    /// </summary>
    /// <returns>Collection of available processor names.</returns>
    public IEnumerable<string> GetAvailableProcessors()
    {
        return _processorTypes.Keys;
    }

    /// <summary>
    /// Discovers processors by scanning assemblies for types that implement IContentProcessor.
    /// </summary>
    private void DiscoverProcessors()
    {
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(),
            Assembly.GetEntryAssembly()
        }.Where(a => a != null).Distinct();

        foreach (var assembly in assemblies)
        {
            try
            {
                var processorTypes = assembly.GetTypes()
                    .Where(t => typeof(IContentProcessor).IsAssignableFrom(t) && 
                               !t.IsAbstract && 
                               !t.IsInterface)
                    .ToList();

                foreach (var type in processorTypes)
                {
                    try
                    {
                        // Create a temporary instance to get the processor name
                        var tempInstance = Activator.CreateInstance(type, _serviceProvider.GetService(typeof(ILogger<>).MakeGenericType(type))) as IContentProcessor;
                        if (tempInstance != null)
                        {
                            _processorTypes[tempInstance.ProcessorName] = type;
                            _logger.LogDebug("Discovered processor: {ProcessorName} ({TypeName})", 
                                tempInstance.ProcessorName, type.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to instantiate processor {TypeName}", type.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to scan assembly {AssemblyName} for processors", assembly?.FullName);
            }
        }

        _logger.LogInformation("Discovered {ProcessorCount} content processors", _processorTypes.Count);
    }
}
