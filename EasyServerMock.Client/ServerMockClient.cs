using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace EasyServerMock.Client;

/// <summary>
/// Create a client to connect to a server mock instance
/// </summary>
public class ServerMockClient : IDisposable
{
    private readonly int _port;
    private IHost? _host;
    private readonly ConcurrentDictionary<string, string> _endpointResponses = new();
    private readonly ConcurrentBag<(string Path, string? Payload)> _requests = new();
    private bool _started = false;

    public ServerMockClient(int port = 7900)
    {
        _port = port;
    }

    public void Start()
    {
        if (_started)
            return;
        _started = true;
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://localhost:{_port}");
        var app = builder.Build();
        app.Run(async context =>
        {
            var path = context.Request.Path.ToString();
            string? payload = null;
            if (context.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(context.Request.Body);
                payload = await reader.ReadToEndAsync();
            }
            _requests.Add((path, payload));
            if (_endpointResponses.TryGetValue(path, out var response))
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(response);
            }
            else
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Not configured");
            }
        });
        _host = app;
        app.StartAsync().GetAwaiter().GetResult();
    }

    public void ConfigureEndpoint(string path, string response)
    {
        _endpointResponses[path] = response;
    }

    public List<(string Path, string? Payload)> GetRequests(string? path = null)
    {
        return _requests.Where(r => path == null || r.Path == path).ToList();
    }

    public void Dispose()
    {
        if (_host != null)
        {
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
            _host = null;
        }
        GC.SuppressFinalize(this);
    }
}
