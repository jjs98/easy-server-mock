using System.Net;

namespace EasyServerMock.Client;

internal class MockResponse
{
    public object Response { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public HttpStatusCode StatusCode { get; set; }

    public MockResponse(object response, Dictionary<string, string> headers)
    {
        Response = response;
        Headers = headers;
        StatusCode = HttpStatusCode.OK;
    }

    public MockResponse(object response, HttpStatusCode statusCode)
    {
        Response = response;
        Headers = [];
        StatusCode = statusCode;
    }

    public MockResponse(
        object response,
        Dictionary<string, string> headers,
        HttpStatusCode statusCode
    )
    {
        Response = response;
        Headers = headers;
        StatusCode = statusCode;
    }
}
