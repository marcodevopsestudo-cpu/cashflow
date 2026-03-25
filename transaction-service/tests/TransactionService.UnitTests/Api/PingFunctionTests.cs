using FluentAssertions;
using System.Net;
using TransactionService.Api.Functions;
using Xunit;

namespace TransactionService.UnitTests.Api;

/// <summary>
/// Unit tests for <see cref="PingFunction"/>.
/// </summary>
public sealed class PingFunctionTests
{
    /// <summary>
    /// Ensures that the Ping endpoint returns HTTP 200 (OK) with a "pong" response body.
    /// </summary>
    [Fact]
    public async Task Run_ShouldReturnOkWithPongBody()
    {
        // Arrange: create fake Azure Function context and HTTP request
        var context = new CreateTransactionFunctionTests.FakeFunctionContext();
        var request = new CreateTransactionFunctionTests.FakeHttpRequestData(context, new { }, HttpMethod.Get.Method);
        var sut = new PingFunction();

        // Act: execute the function
        var response = await sut.Run(request);

        // Assert: validate HTTP status code
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert: validate response body content
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body);
        var body = await reader.ReadToEndAsync();

        body.Should().Be("pong");
    }
}
