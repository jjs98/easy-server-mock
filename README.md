[![GitHub License](https://img.shields.io/github/license/jjs98/easy-server-mock)](LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/EasyServerMock)](https://www.nuget.org/packages/EasyServerMock/)

# EasyServerMock

This is a library to easy mock third party server in your .net integration tests.

## Installation
```powershell
 dotnet add package EasyServerMock
 ```

## Example

Find all the examples in the [tests](./EasyServerMock.IntegrationTests/ServerMockClientTests.cs) project.

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using EasyServerMock;
using FluentAssertions;

public record TestResponse(string Message);

public record TestRequest(string Username);

public class ServerMockClientTests
{
    [Fact]
    public async Task ServerMockClient_ShouldStartOnPort()
    {
        // Arrange
        var client = new ServerMockClient(7901);
        var message = "Hello World";

        await client.StartAsync();
        client.Post("/test").WithResponse(new TestResponse(message)).Provide();
        var httpClient = new HttpClient();
        var request = new TestRequest("Test");
        var json = JsonSerializer.Serialize(request);
        using var httpContent = new StringContent(json);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

        // Act
        var response = await httpClient.PostAsync("http://localhost:7901/test", httpContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<TestResponse>();
        content.Should().NotBeNull();
        content.Message.Should().Be(message);
        var requests = client.GetRequests("/test", HttpMethod.Post);
        requests.Should().HaveCount(1);
        requests[0].Path.Should().Be("/test");
        requests[0].Payload.Should().Be(json);
    }

    [Fact]
    public async Task ServerMockClient_ShouldSaveSendHeaders()
    {
        // Arrange
        var client = new ServerMockClient(7913);
        await client.StartAsync();
        client.Get("/test").WithResponse(new TestResponse("OK")).WithStatusCode(HttpStatusCode.OK).Provide();
        using var httpClient = new HttpClient();
        var guid = Guid.NewGuid().ToString();
        httpClient.DefaultRequestHeaders.Add("X-Request-ID", guid);

        // Act
        var response = await httpClient.GetAsync("http://localhost:7913/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var requests = client.GetRequests("/test", HttpMethod.Get);
        requests.Should().HaveCount(1);
        requests[0].Headers["X-Request-ID"].Should().Be(guid);
    }

    [Fact]
    public async Task ServerMockClient_ShouldReturnHeaders()
    {
        // Arrange
        var client = new ServerMockClient(7914);
        await client.StartAsync();
        var headers = new Dictionary<string, string>
        {
            { "X-Custom-Header", "HeaderValue" },
            { "X-Another-Header", "AnotherValue" }
        };
        client.Get("/test").WithResponse(new TestResponse("OK")).WithStatusCode(HttpStatusCode.OK).WithHeaders(headers).Provide();
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync("http://localhost:7914/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains("X-Custom-Header").Should().BeTrue();
        response.Headers.GetValues("X-Custom-Header").First().Should().Be("HeaderValue");
        response.Headers.Contains("X-Another-Header").Should().BeTrue();
        response.Headers.GetValues("X-Another-Header").First().Should().Be("AnotherValue");
    }
}
```

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing
If you would like to contribute to this project, please fork the repository and submit a pull request. We welcome contributions of all kinds, including bug fixes, new features, and documentation improvements.
