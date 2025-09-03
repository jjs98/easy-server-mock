using System.Net;

namespace EasyServerMock;

public class EndpointBuilder(ServerMockClient client, string path, HttpMethod method)
{
    private object? _responseBody;
    private Dictionary<string, string> _headers = [];
    private HttpStatusCode _statusCode = HttpStatusCode.OK;
    private int _delayMilliseconds = 0;

    /// <summary>
    /// Set the response body for the endpoint
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public EndpointBuilder WithResponse(object response)
    {
        _responseBody = response;
        return this;
    }

    /// <summary>
    /// Set the headers for the endpoint
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    public EndpointBuilder WithHeaders(Dictionary<string, string> headers)
    {
        _headers = headers;
        return this;
    }

    /// <summary>
    /// Set the status code for the endpoint
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public EndpointBuilder WithStatusCode(HttpStatusCode statusCode)
    {
        _statusCode = statusCode;
        return this;
    }

    /// <summary>
    /// Set a delay (in milliseconds) before the response is sent
    /// </summary>
    /// <param name="milliseconds"></param>
    /// <returns></returns>
    public EndpointBuilder WithDelay(int milliseconds)
    {
        _delayMilliseconds = milliseconds;
        return this;
    }

    /// <summary>
    /// Finalize and provide the endpoint to the mock server
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Provide()
    {
        client.ConfigureEndpoint(
            path,
            method,
            _responseBody,
            _headers,
            _statusCode,
            _delayMilliseconds
        );
    }
}
