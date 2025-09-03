using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace EasyServerMock.Client;

/// <summary>
/// Create a client to connect to a server mock instance
/// </summary>
public class ServerMockClient(int port = 7900) : IDisposable
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

                            _requests.Add(new(method, path, payload, headers));
                            if (
                                _endpointResponses.TryGetValue(path, out var mockResponses)
                                && mockResponses.TryGetValue(method, out var mockResponse)
                            )
                            {
                                context.Response.StatusCode = (int)mockResponse.StatusCode;
                                await context.Response.WriteAsJsonAsync(mockResponse.Response);
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

    #region Configure Methods
    /// <summary>
    /// Configure a GET endpoint with a response body and a 200 OK status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    public void ConfigureGet(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Get, responseBody, HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a GET endpoint with a response body and a specific status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    /// <param name="statusCode"></param>
    public void ConfigureGet(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Get, responseBody, statusCode);
    }

    /// <summary>
    /// Configure a POST endpoint with a response body and a 200 OK status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    public void ConfigurePost(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Post, responseBody, HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a POST endpoint with a response body and a specific status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    /// <param name="statusCode"></param>
    public void ConfigurePost(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Post, responseBody, statusCode);
    }

    /// <summary>
    /// Configure a PUT endpoint with a response body and a 200 OK status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    public void ConfigurePut(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Put, responseBody, HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a PUT endpoint with a response body and a specific status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    /// <param name="statusCode"></param>
    public void ConfigurePut(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Put, responseBody, statusCode);
    }

    /// <summary>
    /// Configure a DELETE endpoint with a response body and a 200 OK status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    public void ConfigureDelete(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Delete, responseBody, HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a DELETE endpoint with a response body and a specific status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    /// <param name="statusCode"></param>
    public void ConfigureDelete(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Delete, responseBody, statusCode);
    }

    /// <summary>
    /// Configure a PATCH endpoint with a response body and a 200 OK status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    public void ConfigurePatch(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Patch, responseBody, HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a PATCH endpoint with a response body and a specific status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    /// <param name="statusCode"></param>
    public void ConfigurePatch(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Patch, responseBody, statusCode);
    }

    /// <summary>
    /// Configure a HEAD endpoint with a response body and a 200 OK status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    public void ConfigureHead(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Head, responseBody, HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a HEAD endpoint with a response body and a specific status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    /// <param name="statusCode"></param>
    public void ConfigureHead(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Head, responseBody, statusCode);
    }

    /// <summary>
    /// Configure an OPTIONS endpoint with a response body and a 200 OK status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    public void ConfigureOptions(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Options, responseBody, HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure an OPTIONS endpoint with a response body and a specific status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    /// <param name="statusCode"></param>
    public void ConfigureOptions(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Options, responseBody, statusCode);
    }

    /// <summary>
    /// Configure a TRACE endpoint with a response body and a 200 OK status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    public void ConfigureTrace(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Trace, responseBody, HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a TRACE endpoint with a response body and a specific status code
    /// </summary>
    /// <param name="path"></param>
    /// <param name="responseBody"></param>
    /// <param name="statusCode"></param>
    public void ConfigureTrace(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Trace, responseBody, statusCode);
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

    #endregion

    private void ConfigureEndpoint(
        string path,
        HttpMethod method,
        object responseBody,
        HttpStatusCode statusCode
    )
    {
        if (!_endpointResponses.ContainsKey(path))
        {
            _endpointResponses[path] = [];
        }
        _endpointResponses[path].TryAdd(method, new(responseBody, statusCode));
    }
}
