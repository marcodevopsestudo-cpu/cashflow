using FluentAssertions;
using TransactionService.Api.Configuration;
using TransactionService.Api.Security;
using Xunit;

namespace TransactionService.UnitTests.Api;

/// <summary>
/// Unit tests for <see cref="EntraAuthorizationEvaluator"/>.
/// </summary>
public sealed class EntraAuthorizationEvaluatorTests
{
    private readonly EntraAuthorizationEvaluator _sut = new();

    /// <summary>
    /// Verifies that authorization is allowed when the feature is disabled.
    /// </summary>
    [Fact]
    public void Evaluate_ShouldAllow_WhenFeatureIsDisabled()
    {
        var decision = _sut.Evaluate(
            null,
            new EntraAuthorizationOptions
            {
                Enabled = false
            });

        decision.IsAllowed.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that authorization is denied when the caller principal is missing
    /// and authorization is enabled.
    /// </summary>
    [Fact]
    public void Evaluate_ShouldDeny_WhenPrincipalIsMissing()
    {
        var decision = _sut.Evaluate(
            null,
            new EntraAuthorizationOptions
            {
                Enabled = true
            });

        decision.IsAllowed.Should().BeFalse();
        decision.FailureReason.Should().Be(AuthorizationFailureReason.MissingPrincipal);
    }

    [Fact]
    public void Evaluate_ShouldDeny_WhenAppIdIsNotAllowed()
    {
        var principal = new CallerPrincipal(
            "client-a",
            "tenant-1",
            "api://transaction-service",
            "https://sts.windows.net/tenant-1/",
            ["transactions.write"],
            []);

        var options = new EntraAuthorizationOptions
        {
            Enabled = true,
            AllowedAppIds = ["client-b"]
        };

        var decision = _sut.Evaluate(principal, options);

        decision.IsAllowed.Should().BeFalse();
        decision.FailureReason.Should().Be(AuthorizationFailureReason.AppIdNotAllowed);
    }

    [Fact]
    public void Evaluate_ShouldAllow_WhenAllRulesMatch()
    {
        var principal = new CallerPrincipal(
            "client-a",
            "tenant-1",
            "api://transaction-service",
            "https://sts.windows.net/tenant-1/",
            ["transactions.write"],
            []);

        var options = new EntraAuthorizationOptions
        {
            Enabled = true,
            AllowedAppIds = ["client-a"],
            AllowedAudiences = ["api://transaction-service"],
            AllowedIssuers = ["https://sts.windows.net/tenant-1/"],
            RequiredRoles = ["transactions.write"]
        };

        var decision = _sut.Evaluate(principal, options);

        decision.IsAllowed.Should().BeTrue();
        decision.Principal.Should().NotBeNull();
    }
}
