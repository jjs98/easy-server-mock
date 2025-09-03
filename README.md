[![GitHub License](https://img.shields.io/github/license/jjs98/easy-server-mock?style=for-the-badge)](LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/EasyServerMock?style=for-the-badge)](https://www.nuget.org/packages/EasyServerMock/)
[![Codecov](https://img.shields.io/codecov/c/github/jjs98/easy-server-mock?style=for-the-badge)](https://codecov.io/gh/jjs98/easy-server-mock)

# EasyServerMock

This is a library to easy mock third party server in your .net integration tests.

## Installation
```powershell
 dotnet add package EasyServerMock
 ```

## Configuration

Start a mock server on a specific port.

```csharp
var client = new ServerMockClient(7901);

await client.StartAsync();
```

Configure the mock server to respond to specific requests.

```csharp
client.Post("/test1").WithResponse(new { Message = "Hello World" }).Provide();
```

Configure the mock server to respond with different status codes.

```csharp
client.Get("/test2").WithResponse(new { Message = "Not Found" }).WithStatusCode(HttpStatusCode.NotFound).Provide();
```

Configure the mock server to respond with a delay.

```csharp
client.Get("/test3").WithResponse(new { Message = "TimeOut" }).WithStatusCode(HttpStatusCode.BadRequest).WithDelay(1000).Provide();
```

Configure the mock server to respond with custom headers.

```csharp
var headers = new Dictionary<string, string>
{
    { "X-Custom-Header", "HeaderValue" },
    { "X-Another-Header", "AnotherValue" }
};
client.Get("/test4").WithResponse(new { Message = "Hello World" }).WithHeaders(headers).Provide();
```

## Validation

Retrieve and validate the requests received by the mock server.

```csharp    
var requests = client.GetRequests("/test", HttpMethod.Post);
requests.Should().HaveCount(1);
requests[0].Path.Should().Be("/test");
requests[0].Payload.Should().Be(json);
requests[0].Headers["X-Request-ID"].Should().Be(guid);
requests[0].QueryParameters["user"].Should().Be("test");
requests[0].QueryParameters["lang"].Should().Be("en");
```

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing
If you would like to contribute to this project, please fork the repository and submit a pull request. We welcome contributions of all kinds, including bug fixes, new features, and documentation improvements.
