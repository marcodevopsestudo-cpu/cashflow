using FluentAssertions;
using TransactionService.Api.Common.Constants;
using TransactionService.Api.Common.Extensions;
using Xunit;

namespace TransactionService.UnitTests.Api;

public sealed class FunctionContextExtensionsTests
{
    [Fact]
    public void GetCorrelationId_ShouldReturnStoredValue_WhenAvailable()
    {
        var context = new FakeFunctionContext();
        context.Items[CorrelationConstants.CorrelationIdItemKey] = "corr-123";

        var correlationId = context.GetCorrelationId();

        correlationId.Should().Be("corr-123");
    }

    [Fact]
    public void GetCorrelationId_ShouldGenerateValue_WhenMissing()
    {
        var context = new FakeFunctionContext();

        var correlationId = context.GetCorrelationId();

        correlationId.Should().NotBeNullOrWhiteSpace();
        correlationId.Should().HaveLength(32);
    }
}
