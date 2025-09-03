using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace EasyServerMock;

/// <summary>
/// Create a client to connect to a server mock instance
/// </summary>
public partial class ServerMockClient(int port = 7900) : IDisposable
{
    private IHost? _host;
    private readonly ConcurrentDictionary<
        string,
        ConcurrentDictionary<HttpMethod, MockResponse>
    > _endpointResponses = [];
    private readonly ConcurrentBag<MockRequest> _requests = [];
    private bool _started = false;

    /// <summary>
    /// Start the server mock instance
    /// </summary>
    public async Task StartAsync()
    {
        if (_started)
            return;
        _started = true;
        _host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .UseKestrel()
                    .UseUrls($"http://localhost:{port}")
                    .Configure(app =>
                    {
                        app.Run(async context =>
                        {
                            var path = context.Request.Path.ToString();
                            string? payload = null;
                            if (context.Request.ContentLength > 0)
                            {
                                using var reader = new StreamReader(context.Request.Body);
                                payload = await reader.ReadToEndAsync();
                            }

                            var method = HttpMethod.Parse(context.Request.Method);

                            var headers = context.Request.Headers.ToDictionary(
                                h => h.Key,
                                h => h.Value.ToString()
                            );
                            var queryParameters = context.Request.Query.ToDictionary(
                                q => q.Key,
                                q => q.Value.ToString()
                            );

                            _requests.Add(new(method, path, payload, headers, queryParameters));
                            if (
                                _endpointResponses.TryGetValue(path, out var mockResponses)
                                && mockResponses.TryGetValue(method, out var mockResponse)
                            )
                            {
                                context.Response.StatusCode = (int)mockResponse.StatusCode;
                                foreach (var header in mockResponse.Headers)
                                {
                                    context.Response.Headers[header.Key] = header.Value;
                                }
                                if (mockResponse.Response is not null)
                                {
                                    context.Response.ContentType = "application/json";
                                    await context.Response.WriteAsJsonAsync(mockResponse.Response);
                                }
                                if (mockResponse.DelayMilliseconds > 0)
                                {
                                    await Task.Delay(mockResponse.DelayMilliseconds);
                                }
                            }
                            else
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                await context.Response.WriteAsync("Not configured");
                            }
                        });
                    });
            })
            .Build();
        await _host.StartAsync();
    }

    public void Reset()
    {
        _endpointResponses.Clear();
        _requests.Clear();
    }

    /// <summary>
    /// Disable the server mock instance
    /// </summary>
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

    /// <summary>
    /// Get all requests made to the server mock, optionally filtered by path and/or HTTP method
    /// </summary>
    /// <param name="path"></param>
    /// <param name="httpMethod"></param>
    /// <returns>A List of MockRequests</returns>
    public List<MockRequest> GetRequests(string? path = null, HttpMethod? httpMethod = null)
    {
        return
        [
            .. _requests.Where(r =>
                (path == null || r.Path == path) && (httpMethod == null || r.Method == httpMethod)
            )
        ];
    }

    /// <summary>
    /// Begin configuration of a GET endpoint
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public EndpointBuilder Get(string path) => new(this, path, HttpMethod.Get);

    /// <summary>
    /// Begin configuration of a POST endpoint
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public EndpointBuilder Post(string path) => new(this, path, HttpMethod.Post);

    /// <summary>
    /// Begin configuration of a PUT endpoint
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public EndpointBuilder Put(string path) => new(this, path, HttpMethod.Put);

    /// <summary>
    /// Begin configuration of a DELETE endpoint
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public EndpointBuilder Delete(string path) => new(this, path, HttpMethod.Delete);

    /// <summary>
    /// Begin configuration of a PATCH endpoint
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public EndpointBuilder Patch(string path) => new(this, path, HttpMethod.Patch);

    /// <summary>
    /// Begin configuration of a HEAD endpoint
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public EndpointBuilder Head(string path) => new(this, path, HttpMethod.Head);

    /// <summary>
    /// Begin configuration of a OPTIONS endpoint
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public EndpointBuilder Options(string path) => new(this, path, HttpMethod.Options);

    /// <summary>
    /// Begin configuration of a TRACE endpoint
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public EndpointBuilder Trace(string path) => new(this, path, HttpMethod.Trace);

    internal void ConfigureEndpoint(
        string path,
        HttpMethod method,
        object? responseBody,
        Dictionary<string, string> headers,
        HttpStatusCode statusCode,
        int delayMilliseconds
    )
    {
        if (!_endpointResponses.TryGetValue(path, out _))
        {
            _endpointResponses[path] = [];
        }

        _endpointResponses[path]
            .TryAdd(method, new(responseBody, headers, statusCode, delayMilliseconds));
    }
}
