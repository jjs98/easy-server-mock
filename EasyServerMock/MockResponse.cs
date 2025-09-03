using System.Net;

namespace EasyServerMock;

internal record MockResponse(
    object? Response,
    Dictionary<string, string> Headers,
    HttpStatusCode StatusCode,
    int DelayMilliseconds
);
