using System.Text;
using FluentAssertions;
using TransactionService.Api.Common.Constants;
using TransactionService.Api.Security;
using Xunit;

namespace TransactionService.UnitTests.Api;

public sealed class CallerPrincipalParserTests
{
    [Fact]
    public void Parse_ShouldReturnPrincipal_WhenEasyAuthHeaderExists()
    {
        var context = new FakeFunctionContext();
        var request = new FakeHttpRequestData(context, new { }, HttpMethod.Post.Method);
        request.Headers.Add(AuthorizationConstants.ClientPrincipalHeaderName, Encode("""
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

    [Fact]
    public void Parse_ShouldReturnNull_WhenHeaderDoesNotExist()
    {
        var context = new FakeFunctionContext();
        var request = new FakeHttpRequestData(context, new { }, HttpMethod.Post.Method);

        var principal = CallerPrincipalParser.Parse(request);

        principal.Should().BeNull();
    }

    private static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
}
