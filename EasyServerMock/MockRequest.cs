namespace EasyServerMock;

public record MockRequest(
    HttpMethod Method,
    string Path,
    string? Payload,
    Dictionary<string, string> Headers,
    Dictionary<string, string> QueryParameters
);
