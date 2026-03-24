using System.Text;
using FluentAssertions;
using TransactionService.Api.Common.Constants;
using TransactionService.Api.Security;
using Xunit;
using static TransactionService.UnitTests.Api.CreateTransactionFunctionTests;

namespace TransactionService.UnitTests.Api;

/// <summary>
/// Unit tests for <see cref="CallerPrincipalParser"/>.
/// </summary>
public sealed class CallerPrincipalParserTests
{
    /// <summary>
    /// Verifies that a valid Easy Auth header is correctly parsed into a <see cref="CallerPrincipal"/>.
    /// </summary>
    [Fact]
    public void Parse_ShouldReturnPrincipal_WhenEasyAuthHeaderExists()
    {
        var context = new FakeFunctionContext();
        var request = new FakeHttpRequestData(context, new { }, HttpMethod.Post.Method);

        request.Headers.Add(
            AuthorizationConstants.ClientPrincipalHeaderName,
            Encode("""
            {
              "claims": [
                { "typ": "appid", "val": "client-app-id" },
                { "typ": "tid", "val": "tenant-id" },
                { "typ": "aud", "val": "api://transaction-service" },
                { "typ": "iss", "val": "https://sts.windows.net/tenant-id/" },
                { "typ": "roles", "val": "transactions.write" }
              ]
            }
            """));

        var principal = CallerPrincipalParser.Parse(request);

        principal.Should().NotBeNull();
        principal!.AppId.Should().Be("client-app-id");
        principal.TenantId.Should().Be("tenant-id");
        principal.Audience.Should().Be("api://transaction-service");
        principal.Issuer.Should().Be("https://sts.windows.net/tenant-id/");
        principal.Roles.Should().ContainSingle().Which.Should().Be("transactions.write");
    }

    /// <summary>
    /// Verifies that parsing returns null when the Easy Auth header is not present.
    /// </summary>
    [Fact]
    public void Parse_ShouldReturnNull_WhenHeaderDoesNotExist()
    {
        var context = new FakeFunctionContext();
        var request = new FakeHttpRequestData(context, new { }, HttpMethod.Post.Method);

        var principal = CallerPrincipalParser.Parse(request);

        principal.Should().BeNull();
    }

    /// <summary>
    /// Encodes a string value into Base64 using UTF-8.
    /// </summary>
    /// <param name="value">The string value to encode.</param>
    /// <returns>The Base64-encoded string.</returns>
    private static string Encode(string value)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
}
