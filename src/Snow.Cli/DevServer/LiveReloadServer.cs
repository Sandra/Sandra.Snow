using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Snow.Cli.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace Snow.Cli.DevServer;

/// <summary>
/// WebSocket-based live reload server for development.
/// </summary>
public class LiveReloadServer
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private readonly ILogger<LiveReloadServer> _logger;
    private bool _disposed;

    public LiveReloadServer(ILogger<LiveReloadServer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Configures WebSocket middleware for the application.
    /// </summary>
    /// <param name="app">The web application builder.</param>
    public void ConfigureWebSockets(WebApplication app)
    {
        app.UseWebSockets();

        app.Map("/__sandra/ws", async (HttpContext context) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                // Security: Check origin for WebSocket connections
                var origin = context.Request.Headers["Origin"].FirstOrDefault();
                var isValidOrigin = string.IsNullOrEmpty(origin) || 
                                  origin.StartsWith("http://localhost") || 
                                  origin.StartsWith("http://127.0.0.1");

                if (!isValidOrigin)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Invalid origin");
                    return;
                }

                await HandleWebSocketConnection(context);
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("WebSocket connections only");
            }
        });

        _logger.LogDebug("WebSocket live reload endpoint configured: /__sandra/ws");
    }

    /// <summary>
    /// Handles WebSocket connections from clients.
    /// </summary>
    private async Task HandleWebSocketConnection(HttpContext context)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString();

        _connections[connectionId] = webSocket;
        _logger.LogDebug("Live reload client connected: {ConnectionId}", connectionId);

        try
        {
            // Send initial connection message
            await SendMessage(webSocket, new LiveReloadMessage { Type = "connected" });

            // Keep connection alive and handle incoming messages
            var buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open && !_disposed)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), 
                        CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Closing",
                            CancellationToken.None);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogTrace("Received WebSocket message: {Message}", message);
                        
                        // Handle ping/pong for keep-alive
                        if (message == "ping")
                        {
                            await SendMessage(webSocket, new LiveReloadMessage { Type = "pong" });
                        }
                    }
                }
                catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    // Client disconnected abruptly
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling WebSocket connection: {ConnectionId}", connectionId);
        }
        finally
        {
            _connections.TryRemove(connectionId, out _);
            _logger.LogDebug("Live reload client disconnected: {ConnectionId}", connectionId);
        }
    }

    /// <summary>
    /// Sends a reload message to all connected clients.
    /// </summary>
    /// <param name="changedFiles">Optional list of files that changed.</param>
    public async Task NotifyReloadAsync(IReadOnlyList<string>? changedFiles = null)
    {
        if (_disposed)
            return;

        var reloadMessage = new LiveReloadMessage
        {
            Type = "reload",
            Files = changedFiles?.Select(f => System.Net.WebUtility.HtmlEncode(Path.GetFileName(f))).ToArray()
        };

        var tasks = new List<Task>();

        foreach (var connection in _connections.ToList())
        {
            tasks.Add(SafeSendMessage(connection.Value, reloadMessage, connection.Key));
        }

        if (tasks.Any())
        {
            await Task.WhenAll(tasks);
            _logger.LogDebug("Sent reload notification to {ClientCount} clients", tasks.Count);
        }
    }

    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    /// <param name="message">The message object to send.</param>
    public async Task BroadcastAsync(LiveReloadMessage message)
    {
        if (_disposed)
            return;

        var tasks = new List<Task>();

        foreach (var connection in _connections.ToList())
        {
            tasks.Add(SafeSendMessage(connection.Value, message, connection.Key));
        }

        if (tasks.Any())
        {
            await Task.WhenAll(tasks);
            _logger.LogTrace("Broadcast message to {ClientCount} clients", tasks.Count);
        }
    }

    /// <summary>
    /// Gets the number of active connections.
    /// </summary>
    public int ActiveConnections => _connections.Count;

    /// <summary>
    /// Safely sends a message to a WebSocket, handling errors.
    /// </summary>
    private async Task SafeSendMessage(WebSocket webSocket, LiveReloadMessage message, string connectionId)
    {
        try
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await SendMessage(webSocket, message);
            }
            else
            {
                _connections.TryRemove(connectionId, out _);
            }
        }
        catch (Exception ex)
        {
            _logger.LogTrace(ex, "Failed to send message to client {ConnectionId}", connectionId);
            _connections.TryRemove(connectionId, out _);
        }
    }

    /// <summary>
    /// Sends a JSON message to a WebSocket.
    /// </summary>
    private static async Task SendMessage(WebSocket webSocket, LiveReloadMessage message)
    {
        var json = JsonSerializer.Serialize(message, CliJsonContext.Default.LiveReloadMessage);

        var buffer = Encoding.UTF8.GetBytes(json);
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    /// <summary>
    /// Gets the client-side JavaScript code for live reload.
    /// </summary>
    public static string GetClientScript(string serverUrl)
    {
        return $@"
<!-- Sandra.Snow Live Reload -->
<script>
(function() {{
    const wsUrl = '{serverUrl}/__sandra/ws'.replace('http:', 'ws:').replace('https:', 'wss:');
    let ws;
    let reconnectAttempts = 0;
    const maxReconnectAttempts = 5;

    function connect() {{
        try {{
            ws = new WebSocket(wsUrl);
            
            ws.onopen = function() {{
                console.log('[Sandra.Snow] Live reload connected');
                reconnectAttempts = 0;
                ws.send('ping'); // Initial ping
            }};

            ws.onmessage = function(event) {{
                const data = JSON.parse(event.data);
                
                if (data.type === 'reload') {{
                    console.log('[Sandra.Snow] Reloading page...', data.files || []);
                    window.location.reload();
                }} else if (data.type === 'pong') {{
                    // Keep-alive response
                }}
            }};

            ws.onclose = function() {{
                console.log('[Sandra.Snow] Live reload disconnected');
                
                if (reconnectAttempts < maxReconnectAttempts) {{
                    setTimeout(() => {{
                        reconnectAttempts++;
                        console.log(`[Sandra.Snow] Reconnecting (attempt ${{reconnectAttempts}})...`);
                        connect();
                    }}, 1000 * reconnectAttempts);
                }}
            }};

            ws.onerror = function(error) {{
                console.log('[Sandra.Snow] Live reload error:', error);
            }};

        }} catch (error) {{
            console.log('[Sandra.Snow] Failed to connect to live reload server:', error);
        }}
    }}

    // Start connection
    connect();

    // Cleanup on page unload
    window.addEventListener('beforeunload', function() {{
        if (ws) {{
            ws.close();
        }}
    }});
}})();
</script>
";
    }

    /// <summary>
    /// Disposes the live reload server.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Close all connections
        var closeTasks = new List<Task>();
        foreach (var connection in _connections.Values)
        {
            closeTasks.Add(SafeCloseConnection(connection));
        }

        if (closeTasks.Any())
        {
            Task.WaitAll(closeTasks.ToArray(), TimeSpan.FromSeconds(5));
        }

        _connections.Clear();
        _logger.LogDebug("Live reload server disposed");
    }

    /// <summary>
    /// Safely closes a WebSocket connection.
    /// </summary>
    private async Task SafeCloseConnection(WebSocket webSocket)
    {
        try
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Server shutting down",
                    CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger.LogTrace(ex, "Error closing WebSocket connection");
        }
    }
}
