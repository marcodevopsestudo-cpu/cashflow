using FluentAssertions;
using System.Collections.ObjectModel;
using TransactionService.Api.Configuration;
using TransactionService.Api.Security;
using Xunit;

namespace TransactionService.UnitTests.Api;

public sealed class EntraAuthorizationEvaluatorTests
{
    private readonly EntraAuthorizationEvaluator _sut = new();

    [Fact]
    public void Evaluate_ShouldAllow_WhenFeatureIsDisabled()
    {
        var decision = _sut.Evaluate(null, new EntraAuthorizationOptions { Enabled = false });

        decision.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ShouldDeny_WhenPrincipalIsMissing()
    {
        var decision = _sut.Evaluate(null, new EntraAuthorizationOptions { Enabled = true });

        decision.IsAllowed.Should().BeFalse();
        decision.FailureReason.Should().Be(AuthorizationFailureReason.MissingPrincipal);
    }

    //[Fact]
    //public void Evaluate_ShouldDeny_WhenAppIdIsNotAllowed()
    //{
    //    var principal = new CallerPrincipal(
    //        "client-a",
    //        "tenant-1",
    //        "api://transaction-service",
    //        "https://sts.windows.net/tenant-1/",
    //        ["transactions.write"],
    //        new ReadOnlyCollection<string>(new List<string>()));

    //    var options = new EntraAuthorizationOptions
    //    {
    //        Enabled = true,
    //        AllowedAppIds = ["client-b"]
    //    };

    //    var decision = _sut.Evaluate(principal, options);

    //    decision.IsAllowed.Should().BeFalse();
    //    decision.FailureReason.Should().Be(AuthorizationFailureReason.AppIdNotAllowed);
    //}

    //[Fact]
    //public void Evaluate_ShouldAllow_WhenAllRulesMatch()
    //{
    //    var principal = new CallerPrincipal(
    //        "client-a",
    //        "tenant-1",
    //        "api://transaction-service",
    //        "https://sts.windows.net/tenant-1/",
    //        ["transactions.write"],
    //        new ReadOnlyCollection<string>(new List<string>()));

    //    var options = new EntraAuthorizationOptions
    //    {
    //        Enabled = true,
    //        AllowedAppIds = ["client-a"],
    //        AllowedAudiences = ["api://transaction-service"],
    //        AllowedIssuers = ["https://sts.windows.net/tenant-1/"],
    //        RequiredRoles = ["transactions.write"]
    //    };

    //    var decision = _sut.Evaluate(principal, options);

    //    decision.IsAllowed.Should().BeTrue();
    //    decision.Principal.Should().NotBeNull();
    //}
}
