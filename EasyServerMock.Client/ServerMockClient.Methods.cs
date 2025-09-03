using System.Net;

namespace EasyServerMock.Client;

public partial class ServerMockClient
{
    /// <summary>
    /// Configure a GET endpoint with a response body and a 200 OK status code
    /// </summary>
    public void ConfigureGet(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Get, responseBody, [], HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a GET endpoint with a response body and a specific status code
    /// </summary>
    public void ConfigureGet(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Get, responseBody, [], statusCode);
    }

    /// <summary>
    /// Configure a GET endpoint with a response body, headers and a specific status code
    /// </summary>
    public void ConfigureGet(
        string path,
        object responseBody,
        HttpStatusCode statusCode,
        Dictionary<string, string> headers
    )
    {
        ConfigureEndpoint(path, HttpMethod.Get, responseBody, headers, statusCode);
    }

    /// <summary>
    /// Configure a POST endpoint with a response body and a 200 OK status code
    /// </summary>
    public void ConfigurePost(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Post, responseBody, [], HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a POST endpoint with a response body and a specific status code
    /// </summary>
    public void ConfigurePost(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Post, responseBody, [], statusCode);
    }

    /// <summary>
    /// Configure a POST endpoint with a response body, headers and a specific status code
    /// </summary>
    public void ConfigurePost(
        string path,
        object responseBody,
        HttpStatusCode statusCode,
        Dictionary<string, string> headers
    )
    {
        ConfigureEndpoint(path, HttpMethod.Post, responseBody, headers, statusCode);
    }

    /// <summary>
    /// Configure a PUT endpoint with a response body and a 200 OK status code
    /// </summary>
    public void ConfigurePut(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Put, responseBody, [], HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a PUT endpoint with a response body and a specific status code
    /// </summary>
    public void ConfigurePut(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Put, responseBody, [], statusCode);
    }

    /// <summary>
    /// Configure a PUT endpoint with a response body, headers and a specific status code
    /// </summary>
    public void ConfigurePut(
        string path,
        object responseBody,
        HttpStatusCode statusCode,
        Dictionary<string, string> headers
    )
    {
        ConfigureEndpoint(path, HttpMethod.Put, responseBody, headers, statusCode);
    }

    /// <summary>
    /// Configure a DELETE endpoint with a response body and a 200 OK status code
    /// </summary>
    public void ConfigureDelete(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Delete, responseBody, [], HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a DELETE endpoint with a response body and a specific status code
    /// </summary>
    public void ConfigureDelete(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Delete, responseBody, [], statusCode);
    }

    /// <summary>
    /// Configure a DELETE endpoint with a response body, headers and a specific status code
    /// </summary>
    public void ConfigureDelete(
        string path,
        object responseBody,
        HttpStatusCode statusCode,
        Dictionary<string, string> headers
    )
    {
        ConfigureEndpoint(path, HttpMethod.Delete, responseBody, headers, statusCode);
    }

    /// <summary>
    /// Configure a PATCH endpoint with a response body and a 200 OK status code
    /// </summary>
    public void ConfigurePatch(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Patch, responseBody, [], HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a PATCH endpoint with a response body and a specific status code
    /// </summary>
    public void ConfigurePatch(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Patch, responseBody, [], statusCode);
    }

    /// <summary>
    /// Configure a PATCH endpoint with a response body, headers and a specific status code
    /// </summary>
    public void ConfigurePatch(
        string path,
        object responseBody,
        HttpStatusCode statusCode,
        Dictionary<string, string> headers
    )
    {
        ConfigureEndpoint(path, HttpMethod.Patch, responseBody, headers, statusCode);
    }

    /// <summary>
    /// Configure a HEAD endpoint with a response body and a 200 OK status code
    /// </summary>
    public void ConfigureHead(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Head, responseBody, [], HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a HEAD endpoint with a response body and a specific status code
    /// </summary>
    public void ConfigureHead(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Head, responseBody, [], statusCode);
    }

    /// <summary>
    /// Configure a HEAD endpoint with a response body, headers and a specific status code
    /// </summary>
    public void ConfigureHead(
        string path,
        object responseBody,
        HttpStatusCode statusCode,
        Dictionary<string, string> headers
    )
    {
        ConfigureEndpoint(path, HttpMethod.Head, responseBody, headers, statusCode);
    }

    /// <summary>
    /// Configure an OPTIONS endpoint with a response body and a 200 OK status code
    /// </summary>
    public void ConfigureOptions(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Options, responseBody, [], HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure an OPTIONS endpoint with a response body and a specific status code
    /// </summary>
    public void ConfigureOptions(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Options, responseBody, [], statusCode);
    }

    /// <summary>
    /// Configure an OPTIONS endpoint with a response body, headers and a specific status code
    /// </summary>
    public void ConfigureOptions(
        string path,
        object responseBody,
        HttpStatusCode statusCode,
        Dictionary<string, string> headers
    )
    {
        ConfigureEndpoint(path, HttpMethod.Options, responseBody, headers, statusCode);
    }

    /// <summary>
    /// Configure a TRACE endpoint with a response body and a 200 OK status code
    /// </summary>
    public void ConfigureTrace(string path, object responseBody)
    {
        ConfigureEndpoint(path, HttpMethod.Trace, responseBody, [], HttpStatusCode.OK);
    }

    /// <summary>
    /// Configure a TRACE endpoint with a response body and a specific status code
    /// </summary>
    public void ConfigureTrace(string path, object responseBody, HttpStatusCode statusCode)
    {
        ConfigureEndpoint(path, HttpMethod.Trace, responseBody, [], statusCode);
    }

    /// <summary>
    /// Configure a TRACE endpoint with a response body, headers and a specific status code
    /// </summary>
    public void ConfigureTrace(
        string path,
        object responseBody,
        HttpStatusCode statusCode,
        Dictionary<string, string> headers
    )
    {
        ConfigureEndpoint(path, HttpMethod.Trace, responseBody, headers, statusCode);
    }
}
