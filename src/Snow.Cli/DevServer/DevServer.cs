using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Net;
using Spectre.Console;

namespace Snow.Cli.DevServer;

/// <summary>
/// Development web server for serving static sites with live reload capabilities.
/// </summary>
public class DevServer : IDisposable, IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly string _contentPath;
    private readonly int _port;
    private readonly ILogger<DevServer> _logger;
    private readonly LiveReloadServer? _liveReloadServer;

    private DevServer(WebApplication app, string contentPath, int port, ILogger<DevServer> logger, LiveReloadServer? liveReloadServer = null)
    {
        _app = app;
        _contentPath = contentPath;
        _port = port;
        _logger = logger;
        _liveReloadServer = liveReloadServer;
    }

    /// <summary>
    /// Creates and starts a development server.
    /// </summary>
    /// <param name="contentPath">Path to the generated static site content.</param>
    /// <param name="port">Port to listen on (0 for auto-assign).</param>
    /// <param name="enableDirectoryBrowsing">Whether to enable directory browsing.</param>
    /// <param name="enableLiveReload">Whether to enable live reload functionality.</param>
    /// <param name="autoOpen">Whether to automatically open browser.</param>
    /// <param name="cancellationToken">Cancellation token for startup.</param>
    /// <returns>A running development server instance.</returns>
    public static async Task<DevServer> StartAsync(
        string contentPath, 
        int port = 0, 
        bool enableDirectoryBrowsing = true,
        bool enableLiveReload = false,
        bool autoOpen = true,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(contentPath))
        {
            throw new DirectoryNotFoundException($"Content directory not found: {contentPath}");
        }

        // Find available port if not specified (0 = auto-assign)
        var actualPort = port > 0 ? port : FindAvailablePort(5498);
        
        var builder = WebApplication.CreateBuilder();
        
        // Configure services
        builder.Services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning); // Quiet by default
        });

        // Configure Kestrel
        builder.WebHost.UseKestrel(options =>
        {
            options.ListenLocalhost(actualPort);
        });

        var app = builder.Build();
        var logger = app.Services.GetRequiredService<ILogger<DevServer>>();

        // Setup live reload if enabled
        LiveReloadServer? liveReloadServer = null;
        if (enableLiveReload)
        {
            var liveReloadLogger = app.Services.GetRequiredService<ILogger<LiveReloadServer>>();
            liveReloadServer = new LiveReloadServer(liveReloadLogger);
            liveReloadServer.ConfigureWebSockets(app);
        }

        // Configure middleware
        ConfigureMiddleware(app, contentPath, enableDirectoryBrowsing, enableLiveReload, logger);

        // Start the server
        await app.StartAsync(cancellationToken);
        
        var serverUrl = $"http://localhost:{actualPort}";
        
        AnsiConsole.MarkupLine($"[green]Development server started[/]");
        AnsiConsole.MarkupLine($"[cyan]Local:[/] {serverUrl}");
        AnsiConsole.MarkupLine($"[dim]Serving:[/] {Path.GetFullPath(contentPath)}");
        
        if (enableDirectoryBrowsing)
        {
            AnsiConsole.MarkupLine("[dim]Directory browsing:[/] enabled");
        }

        // Auto-open browser
        if (autoOpen)
        {
            try
            {
                OpenBrowser(serverUrl);
                AnsiConsole.MarkupLine("[dim]Browser opened automatically[/]");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to open browser automatically");
            }
        }

        return new DevServer(app, contentPath, actualPort, logger, liveReloadServer);
    }

    /// <summary>
    /// Configures the middleware pipeline for the development server.
    /// </summary>
    private static void ConfigureMiddleware(WebApplication app, string contentPath, bool enableDirectoryBrowsing, bool enableLiveReload, ILogger logger)
    {
        // Development-friendly caching
        app.Use(async (context, next) =>
        {
            // Disable caching for development
            context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            context.Response.Headers["Pragma"] = "no-cache"; 
            context.Response.Headers["Expires"] = "0";
            
            await next();
        });

        var fileProvider = new PhysicalFileProvider(Path.GetFullPath(contentPath));
        var absoluteContentPath = Path.GetFullPath(contentPath);

        // 1. Handle clean URLs for static sites (e.g., /2013/01/post-slug → /2013/01/post-slug/index.html)
        // This must come BEFORE UseDefaultFiles/UseStaticFiles to rewrite the path first
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value ?? "";
            
            // If path has no extension and doesn't end with /, check if directory/index.html exists
            if (!string.IsNullOrEmpty(path) && !Path.HasExtension(path) && !path.EndsWith('/'))
            {
                var indexPath = path + "/index.html";
                var fullPath = Path.Combine(absoluteContentPath, indexPath.TrimStart('/'));
                
                if (File.Exists(fullPath))
                {
                    // Rewrite path to include index.html
                    context.Request.Path = indexPath;
                }
            }
            
            await next();
        });

        // 2. Default files (changes path /dir/ → /dir/index.html)
        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = fileProvider,
            DefaultFileNames = ["index.html", "index.htm", "default.html"]
        });

        // 3. Static file serving
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = fileProvider,
            ContentTypeProvider = CreateContentTypeProvider(),
            ServeUnknownFileTypes = true, // Allow unknown file types for development
            DefaultContentType = "application/octet-stream",
            OnPrepareResponse = context =>
            {
                // Additional no-cache headers for development
                context.Context.Response.Headers["Cache-Control"] = "no-store";
            }
        });

        // 4. Directory browsing (development convenience)
        if (enableDirectoryBrowsing)
        {
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = fileProvider
            });
        }

        // Live reload script injection (if enabled) - must be before static files
        if (enableLiveReload)
        {
            app.Use(async (context, next) =>
            {
                // Capture response body for HTML injection
                var originalBody = context.Response.Body;
                using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                await next();

                memoryStream.Seek(0, SeekOrigin.Begin);
                var content = await new StreamReader(memoryStream).ReadToEndAsync();

                // Inject live reload script into HTML responses
                if (context.Response.ContentType?.Contains("text/html") == true &&
                    context.Response.StatusCode == 200 &&
                    content.Contains("</body>", StringComparison.OrdinalIgnoreCase))
                {
                    var serverUrl = $"{context.Request.Scheme}://{context.Request.Host}";
                    var script = LiveReloadServer.GetClientScript(serverUrl);
                    content = content.Replace("</body>", script + "</body>", StringComparison.OrdinalIgnoreCase);
                }

                context.Response.Body = originalBody;
                await context.Response.WriteAsync(content);
            });
        }

        logger.LogDebug("Configured development server middleware for: {ContentPath} (LiveReload: {LiveReload})", 
            contentPath, enableLiveReload);
    }

    /// <summary>
    /// Creates a content type provider with web development-friendly MIME types.
    /// </summary>
    private static FileExtensionContentTypeProvider CreateContentTypeProvider()
    {
        var provider = new FileExtensionContentTypeProvider();
        
        // Add additional MIME types for modern web development
        provider.Mappings[".webmanifest"] = "application/manifest+json";
        provider.Mappings[".json"] = "application/json";
        provider.Mappings[".wasm"] = "application/wasm";
        provider.Mappings[".br"] = "application/brotli";
        
        return provider;
    }

    /// <summary>
    /// Finds an available port starting from the specified port.
    /// </summary>
    private static int FindAvailablePort(int startingPort)
    {
        for (int port = startingPort; port < startingPort + 100; port++)
        {
            try
            {
                using var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return port;
            }
            catch (SocketException)
            {
                // Port is in use, try next one
            }
        }

        throw new InvalidOperationException($"No available ports found in range {startingPort}-{startingPort + 99}");
    }

    /// <summary>
    /// Opens the default browser to the specified URL.
    /// </summary>
    private static void OpenBrowser(string url)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (OperatingSystem.IsMacOS())
            {
                System.Diagnostics.Process.Start("open", url);
            }
            else if (OperatingSystem.IsLinux())
            {
                System.Diagnostics.Process.Start("xdg-open", url);
            }
        }
        catch
        {
            // Ignore browser opening failures
        }
    }

    /// <summary>
    /// Gets the server URL.
    /// </summary>
    public string ServerUrl => $"http://localhost:{_port}";

    /// <summary>
    /// Notifies all connected clients to reload (for live reload functionality).
    /// </summary>
    /// <param name="changedFiles">Optional list of files that changed.</param>
    public async Task NotifyReloadAsync(IReadOnlyList<string>? changedFiles = null)
    {
        if (_liveReloadServer != null)
        {
            await _liveReloadServer.NotifyReloadAsync(changedFiles);
        }
    }

    /// <summary>
    /// Gets the number of active live reload connections.
    /// </summary>
    public int ActiveConnections => _liveReloadServer?.ActiveConnections ?? 0;

    /// <summary>
    /// Waits for the server to shut down.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for shutdown.</param>
    public async Task WaitForShutdownAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _app.WaitForShutdownAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Development server shutdown requested");
        }
    }

    /// <summary>
    /// Stops the development server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for shutdown.</param>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _app.StopAsync(cancellationToken);
        _logger.LogInformation("Development server stopped");
    }

    /// <summary>
    /// Disposes the development server asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _liveReloadServer?.Dispose();
        
        if (_app != null)
        {
            await _app.DisposeAsync();
        }
        
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the development server synchronously.
    /// </summary>
    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
        GC.SuppressFinalize(this);
    }
}
