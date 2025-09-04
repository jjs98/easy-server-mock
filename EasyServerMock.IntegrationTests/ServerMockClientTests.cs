using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using FluentAssertions;

namespace EasyServerMock.IntegrationTests;

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
        var response = await httpClient.PostAsync(
            "http://localhost:7901/test",
            httpContent,
            TestContext.Current.CancellationToken
        );

        // Assert
        await ValidateResponse(response, message);
        var requests = client.GetRequests("/test", HttpMethod.Post);
        requests.Should().HaveCount(1);
        requests[0].Path.Should().Be("/test");
        requests[0].Payload.Should().Be(json);
    }

    [Fact]
    public async Task ServerMockClient_ShouldReturn404_WhenEndpointNotConfigured()
    {
        // Arrange
        var client = new ServerMockClient(7902);
        await client.StartAsync();
        var httpClient = new HttpClient();
        var request = new TestRequest("Test");
        var json = JsonSerializer.Serialize(request);
        using var httpContent = new StringContent(json);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

        // Act
        var response = await httpClient.PostAsync(
            "http://localhost:7902/unconfigured",
            httpContent,
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        content.Should().Be("Not configured");
        var requests = client.GetRequests("/unconfigured");
        requests.Should().HaveCount(1);
        requests[0].Path.Should().Be("/unconfigured");
        requests[0].Payload.Should().Be(json);
    }

    [Fact]
    public async Task ServerMockClient_ShouldNotStartMultipleTimes()
    {
        // Arrange
        var client = new ServerMockClient(7903);

        // Act
        await client.StartAsync();
        await client.StartAsync(); // Should not throw or start multiple times

        // Assert
        // If no exception is thrown, the test passes
    }

    [Fact]
    public async Task ServerMockClient_ShouldDisposeProperly()
    {
        // Arrange
        var client = new ServerMockClient(7904);
        await client.StartAsync();

        // Act
        await client.DisposeAsync();

        // Assert
        // If no exception is thrown, the test passes
    }

    [Fact]
    public async Task ServerMockClient_ShouldConfigureMultipleEndpoints()
    {
        // Arrange
        var response1Body = new TestResponse("Response 1");
        var response2Body = new TestResponse("Response 2");
        var client = new ServerMockClient(7905);
        await client.StartAsync();
        client.Get("/test1").WithResponse(response1Body).Provide();
        client.Get("/test2").WithResponse(response2Body).Provide();

        // Act
        using var httpClient = new HttpClient();
        var response1 = await httpClient.GetAsync(
            "http://localhost:7905/test1",
            TestContext.Current.CancellationToken
        );
        var response2 = await httpClient.GetAsync(
            "http://localhost:7905/test2",
            TestContext.Current.CancellationToken
        );

        // Assert
        var requests = client.GetRequests(HttpMethod.Get);
        requests.Should().HaveCount(2);
        await ValidateResponse(response1, "Response 1");
        await ValidateResponse(response2, "Response 2");
    }

    [Fact]
    public async Task ServerMockClient_ShouldConfigureMultipleServer()
    {
        // Arrange
        var response1Body = new TestResponse("Response 1");
        var response2Body = new TestResponse("Response 2");
        var client1 = new ServerMockClient(7906);
        var client2 = new ServerMockClient(7907);
        await client1.StartAsync();
        await client2.StartAsync();
        client1.Get("/test").WithResponse(response1Body).Provide();
        client2.Get("/test").WithResponse(response2Body).Provide();

        // Act
        using var httpClient = new HttpClient();
        var response1 = await httpClient.GetAsync(
            "http://localhost:7906/test",
            TestContext.Current.CancellationToken
        );
        var response2 = await httpClient.GetAsync(
            "http://localhost:7907/test",
            TestContext.Current.CancellationToken
        );

        // Assert
        var requests1 = client1.GetRequests();
        requests1.Should().HaveCount(1);
        var requests2 = client2.GetRequests();
        requests2.Should().HaveCount(1);
        await ValidateResponse(response1, "Response 1");
        await ValidateResponse(response2, "Response 2");
    }

    [Fact]
    public async Task ServerMockClient_ShouldReturnConfiguredStatusCode()
    {
        // Arrange
        var client = new ServerMockClient(7908);
        await client.StartAsync();
        client
            .Get("/test")
            .WithResponse(new TestResponse("Created"))
            .WithStatusCode(HttpStatusCode.Created)
            .Provide();
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync(
            "http://localhost:7908/test",
            TestContext.Current.CancellationToken
        );

        // Assert
        await ValidateResponse(response, "Created", HttpStatusCode.Created);
    }

    [Fact]
    public async Task ServerMockClient_ShouldHandleMultipleRequests()
    {
        // Arrange
        var client = new ServerMockClient(7909);
        await client.StartAsync();
        client.Get("/test").WithResponse(new TestResponse("OK")).Provide();
        using var httpClient = new HttpClient();

        // Act
        var tasks = Enumerable
            .Range(0, 10)
            .Select(async _ =>
                await httpClient.GetAsync(
                    "http://localhost:7909/test",
                    TestContext.Current.CancellationToken
                )
            );
        await Task.WhenAll(tasks);

        // Assert
        var requests = client.GetRequests("/test");
        requests.Should().HaveCount(10);
        foreach (var request in requests)
        {
            request.Path.Should().Be("/test");
            request.Payload.Should().BeNull();
        }
    }

    [Fact]
    public async Task ServerMockClient_ShouldNotRunOnUsedPort()
    {
        // Arrange
        var client1 = new ServerMockClient(7910);
        await client1.StartAsync();
        var client2 = new ServerMockClient(7910);

        // Act
        Func<Task> act = async () => await client2.StartAsync();

        // Assert
        await act.Should().ThrowAsync<IOException>();
    }

    [Fact]
    public async Task ServerMockClient_ShouldClearRequests()
    {
        // Arrange
        var client = new ServerMockClient(7911);
        await client.StartAsync();
        client.Get("/test").WithResponse(new TestResponse("OK")).Provide();
        using var httpClient = new HttpClient();

        // Act
        await httpClient.GetAsync(
            "http://localhost:7911/test",
            TestContext.Current.CancellationToken
        );
        var requestsBeforeClear = client.GetRequests("/test");
        client.Reset();
        var requestsAfterClear = client.GetRequests("/test");

        // Assert
        requestsBeforeClear.Should().HaveCount(1);
        requestsAfterClear.Should().BeEmpty();
    }

    [Fact]
    public async Task ServerMockClient_ShouldHandleDifferentHttpMethods()
    {
        // Arrange
        var client = new ServerMockClient(7912);
        await client.StartAsync();

        client.Get("/test").WithResponse(new TestResponse("GET OK")).Provide();
        client.Post("/test").WithResponse(new TestResponse("POST OK")).Provide();
        client.Patch("/test").WithResponse(new TestResponse("PATCH OK")).Provide();
        client.Put("/test").WithResponse(new TestResponse("PUT OK")).Provide();
        client.Delete("/test").WithResponse(new TestResponse("DELETE OK")).Provide();
        client.Head("/test").WithResponse(new TestResponse("HEAD OK")).Provide();
        client.Options("/test").WithResponse(new TestResponse("OPTIONS OK")).Provide();
        client.Trace("/test").WithResponse(new TestResponse("TRACE OK")).Provide();

        using var httpClient = new HttpClient();

        // Act
        var getResponse = await httpClient.GetAsync(
            "http://localhost:7912/test",
            TestContext.Current.CancellationToken
        );
        var postResponse = await httpClient.PostAsync(
            "http://localhost:7912/test",
            null,
            TestContext.Current.CancellationToken
        );
        var patchResponse = await httpClient.PatchAsync(
            "http://localhost:7912/test",
            null,
            TestContext.Current.CancellationToken
        );
        var putResponse = await httpClient.PutAsync(
            "http://localhost:7912/test",
            null,
            TestContext.Current.CancellationToken
        );
        var deleteResponse = await httpClient.DeleteAsync(
            "http://localhost:7912/test",
            TestContext.Current.CancellationToken
        );
        var headRequest = new HttpRequestMessage(HttpMethod.Head, "http://localhost:7912/test");
        var headResponse = await httpClient.SendAsync(
            headRequest,
            TestContext.Current.CancellationToken
        );
        var optionsRequest = new HttpRequestMessage(
            HttpMethod.Options,
            "http://localhost:7912/test"
        );
        var optionsResponse = await httpClient.SendAsync(
            optionsRequest,
            TestContext.Current.CancellationToken
        );
        var traceRequest = new HttpRequestMessage(HttpMethod.Trace, "http://localhost:7912/test");
        var traceResponse = await httpClient.SendAsync(
            traceRequest,
            TestContext.Current.CancellationToken
        );

        // Assert
        await ValidateResponse(getResponse, "GET OK");
        await ValidateResponse(postResponse, "POST OK");
        await ValidateResponse(patchResponse, "PATCH OK");
        await ValidateResponse(putResponse, "PUT OK");
        await ValidateResponse(deleteResponse, "DELETE OK");
        await ValidateResponse(headResponse, "HEAD OK", expectJson: false);
        await ValidateResponse(optionsResponse, "OPTIONS OK");
        await ValidateResponse(traceResponse, "TRACE OK");

        var requests = client.GetRequests("/test");
        requests.Should().HaveCount(8);
        requests.Count(r => r.Method == HttpMethod.Get).Should().Be(1);
        requests.Count(r => r.Method == HttpMethod.Post).Should().Be(1);
        requests.Count(r => r.Method == HttpMethod.Patch).Should().Be(1);
        requests.Count(r => r.Method == HttpMethod.Put).Should().Be(1);
        requests.Count(r => r.Method == HttpMethod.Delete).Should().Be(1);
        requests.Count(r => r.Method == HttpMethod.Head).Should().Be(1);
        requests.Count(r => r.Method == HttpMethod.Options).Should().Be(1);
        requests.Count(r => r.Method == HttpMethod.Trace).Should().Be(1);
    }

    [Fact]
    public async Task ServerMockClient_ShouldSaveSendHeaders()
    {
        // Arrange
        var client = new ServerMockClient(7913);
        await client.StartAsync();
        client.Get("/test").WithResponse(new TestResponse("OK")).Provide();
        using var httpClient = new HttpClient();
        var guid = Guid.NewGuid().ToString();
        httpClient.DefaultRequestHeaders.Add("X-Request-ID", guid);

        // Act
        var response = await httpClient.GetAsync(
            "http://localhost:7913/test",
            TestContext.Current.CancellationToken
        );

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
        client.Get("/test").WithResponse(new TestResponse("OK")).WithHeaders(headers).Provide();
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync(
            "http://localhost:7914/test",
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains("X-Custom-Header").Should().BeTrue();
        response.Headers.GetValues("X-Custom-Header").First().Should().Be("HeaderValue");
        response.Headers.Contains("X-Another-Header").Should().BeTrue();
        response.Headers.GetValues("X-Another-Header").First().Should().Be("AnotherValue");
    }

    [Fact]
    public async Task ServerMockClient_ShouldConfigureQueryParameter()
    {
        // Arrange
        var client = new ServerMockClient(7915);
        await client.StartAsync();
        client.Get("/test").WithResponse(new TestResponse("OK")).Provide();
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync(
            "http://localhost:7915/test?user=test&lang=en",
            TestContext.Current.CancellationToken
        );

        // Assert
        await ValidateResponse(response, "OK");
        var requests = client.GetRequests("/test", HttpMethod.Get);
        requests.Should().HaveCount(1);
        requests[0].QueryParameters["user"].Should().Be("test");
        requests[0].QueryParameters["lang"].Should().Be("en");
    }

    [Fact]
    public async Task ServerMockClient_ShouldHandleNoContentResponse()
    {
        // Arrange
        var client = new ServerMockClient(7916);
        await client.StartAsync();
        client.Get("/test").Provide();
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync(
            "http://localhost:7916/test",
            TestContext.Current.CancellationToken
        );

        // Assert
        await ValidateResponse(response, string.Empty, expectJson: false);
        var requests = client.GetRequests("/test", HttpMethod.Get);
        requests.Should().HaveCount(1);
    }

    [Fact]
    public async Task ServerMockClient_ShouldHandleDelay()
    {
        // Arrange
        var client = new ServerMockClient(7917);
        await client.StartAsync();
        client.Get("/test").WithResponse(new TestResponse("OK")).WithDelay(200).Provide();
        using var httpClient = new HttpClient();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await httpClient.GetAsync(
            "http://localhost:7917/test",
            TestContext.Current.CancellationToken
        );

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(200);
        await ValidateResponse(response, "OK");
        var requests = client.GetRequests("/test", HttpMethod.Get);
        requests.Should().HaveCount(1);
    }

    [Fact]
    public async Task ServerMockClient_ShouldReturnEmptyResponses_WhenNoCallsMade()
    {
        // Arrange
        var client = new ServerMockClient(7918);
        await client.StartAsync();
        client.Get("/test").WithResponse(new TestResponse("OK")).Provide();

        // Act
        // Assert
        client.GetRequests("/test", HttpMethod.Get).Should().HaveCount(0);
        client.GetRequests("/test").Should().HaveCount(0);
        client.GetRequests(httpMethod: HttpMethod.Get).Should().HaveCount(0);
        client.GetRequests().Should().HaveCount(0);
    }

    [Fact]
    public async Task ServerMockClient_ShouldReturnEmptyResponses_WhenDifferentCallsMade()
    {
        // Arrange
        var client = new ServerMockClient(7919);
        await client.StartAsync();
        client.Get("/test").WithResponse(new TestResponse("OK")).Provide();
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync(
            "http://localhost:7919/test",
            TestContext.Current.CancellationToken
        );

        // Assert
        client.GetRequests("/test1", HttpMethod.Post).Should().HaveCount(0);
        client.GetRequests("/test1").Should().HaveCount(0);
        client.GetRequests(HttpMethod.Post).Should().HaveCount(0);
        client.GetRequests().Should().HaveCount(1);
    }

    [Fact]
    public async Task ServerMockClient_ShouldRestartServer_WhenStartCalledAfterDispose()
    {
        // Arrange
        var client = new ServerMockClient(7920);
        await client.StartAsync();
        client.Get("/test").WithResponse(new TestResponse("OK")).Provide();
        using var httpClient = new HttpClient();

        // Act
        await client.DisposeAsync();
        await client.StartAsync();

        var response = await httpClient.GetAsync(
            "http://localhost:7920/test",
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        client.GetRequests("/test", HttpMethod.Get).Should().HaveCount(1);
    }

    private static async Task ValidateResponse(
        HttpResponseMessage response,
        string expectedMessage,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        bool expectJson = true
    )
    {
        response.StatusCode.Should().Be(statusCode);
        if (expectJson)
        {
            var content = await response.Content.ReadFromJsonAsync<TestResponse>(
                TestContext.Current.CancellationToken
            );
            content.Should().NotBeNull();
            content.Message.Should().Be(expectedMessage);
        }
        else
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().BeEmpty();
        }
    }
}
