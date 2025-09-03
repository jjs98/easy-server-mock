namespace EasyServerMock.Client;

public record MockRequest(
    HttpMethod Method,
    string Path,
    string? Payload,
    Dictionary<string, string> Headers
);
