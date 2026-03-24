using FluentAssertions;
using TransactionService.Api.Common.Constants;
using TransactionService.Api.Common.Extensions;
using Xunit;
using static TransactionService.UnitTests.Api.CreateTransactionFunctionTests;

namespace TransactionService.UnitTests.Api;

/// <summary>
/// Unit tests for FunctionContext extension methods.
/// </summary>
public sealed class FunctionContextExtensionsTests
{
    /// <summary>
    /// Verifies that the stored correlation ID is returned when present in the context.
    /// </summary>
    [Fact]
    public void GetCorrelationId_ShouldReturnStoredValue_WhenAvailable()
    {
        // Arrange
        var context = new FakeFunctionContext();
        context.Items[CorrelationConstants.CorrelationIdItemKey] = "corr-123";

        // Act
        var correlationId = context.GetCorrelationId();

        // Assert
        correlationId.Should().Be("corr-123");
    }

    /// <summary>
    /// Verifies that a new correlation ID is generated when none exists in the context.
    /// </summary>
    [Fact]
    public void GetCorrelationId_ShouldGenerateValue_WhenMissing()
    {
        // Arrange
        var context = new FakeFunctionContext();

        // Act
        var correlationId = context.GetCorrelationId();

        // Assert
        correlationId.Should().NotBeNullOrWhiteSpace();
        correlationId.Should().HaveLength(32); // GUID "N" format
    }
}
