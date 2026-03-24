using Azure.Core.Serialization;
using FluentAssertions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TransactionService.Api.Common.Constants;
using TransactionService.Api.Common.Extensions;
using TransactionService.Api.Contracts.Requests;
using TransactionService.Api.Functions;
using TransactionService.Application.Transactions.Commands.CreateTransaction;
using TransactionService.Application.Transactions.Common;
using TransactionService.Domain.Enums;
using Xunit;

namespace TransactionService.UnitTests.Api;

/// <summary>
/// Unit tests for <see cref="CreateTransactionFunction"/>.
/// </summary>
public sealed class CreateTransactionFunctionTests
{
    /// <summary>
    /// Verifies that when an idempotent request is replayed,
    /// the response contains the appropriate headers.
    /// </summary>
    [Fact]

    public async Task Run_ShouldMarkResponseWhenIdempotencyWasReplayed()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var now = DateTime.UtcNow;
        var transactionId = Guid.NewGuid();

        mediator
            .Setup(x => x.Send(It.IsAny<CreateTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateTransactionResult(
                new TransactionDto(
                    transactionId,
                    "ACC-001",
                    TransactionKind.Credit,
                    100m,
                    "BRL",
                    now,
                    "Initial deposit",
                    "Received",
                    "corr-1",
                    now,
                    null),
                true));

        var function = new CreateTransactionFunction(
            mediator.Object,
            NullLogger<CreateTransactionFunction>.Instance);

        var context = new FakeFunctionContext();
        context.Items[CorrelationConstants.CorrelationIdItemKey] = "corr-1";
        context.SetIdempotencyKey("idem-1");

        var requestPayload = new CreateTransactionRequest
        {
            AccountId = "ACC-001",
            Kind = "Credit",
            Amount = 100m,
            Currency = "BRL",
            TransactionDateUtc = now,
            Description = "Initial deposit"
        };

        var request = new FakeHttpRequestData(context, requestPayload, HttpMethod.Post.Method);

        // Act
        var response = await function.Run(request, context, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Headers
            .TryGetValues(IdempotencyConstants.IdempotencyReplayedHeaderName, out var replayedHeader)
            .Should().BeTrue();

        replayedHeader!.Single().Should().Be("true");

        response.Headers
            .TryGetValues(IdempotencyConstants.IdempotencyHeaderName, out var idempotencyValues)
            .Should().BeTrue();

        idempotencyValues!.Single().Should().Be("idem-1");

        response.Body.Position = 0;

        var payload = await JsonSerializer.DeserializeAsync<CreateTransactionResult>(
            response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        payload.Should().NotBeNull();
        payload!.Transaction.Id.Should().Be(transactionId);
        payload.Transaction.AccountId.Should().Be("ACC-001");

        
    }
    /// <summary>
    /// Fake implementation of <see cref="FunctionContext"/> for testing purposes.
    /// </summary>
    internal sealed class FakeFunctionContext : FunctionContext
    {
        private IDictionary<object, object> _items = new Dictionary<object, object>();
        private IServiceProvider _instanceServices;

        public FakeFunctionContext()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddSingleton<IOptions<WorkerOptions>>(Options.Create(new WorkerOptions
            {
                Serializer = new JsonObjectSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web))
            }));

            _instanceServices = services.BuildServiceProvider();
        }

        public override string InvocationId { get; } = Guid.NewGuid().ToString("N");
        public override string FunctionId { get; } = Guid.NewGuid().ToString("N");
        public override TraceContext TraceContext { get; } = null!;
        public override BindingContext BindingContext { get; } = null!;
        public override RetryContext RetryContext { get; } = null!;
        public override IServiceProvider InstanceServices { get => _instanceServices; set => _instanceServices = value; }
        public override FunctionDefinition FunctionDefinition { get; } = null!;
        public override IDictionary<object, object> Items { get => _items; set => _items = value; }
        public override IInvocationFeatures Features { get; } = null!;
        public override CancellationToken CancellationToken => CancellationToken.None;
    }

    /// <summary>
    /// Fake implementation of <see cref="HttpRequestData"/> for testing purposes.
    /// </summary>
    internal sealed class FakeHttpRequestData : HttpRequestData
    {
        private readonly MemoryStream _body;

        public FakeHttpRequestData(FunctionContext functionContext, object payload, string method)
            : base(functionContext)
        {
            var json = JsonSerializer.Serialize(payload);
            _body = new MemoryStream(Encoding.UTF8.GetBytes(json));

            Headers = [];
            Method = method;
            Url = new Uri("https://localhost/api/transactions");
            Cookies = [];
            Identities = [];
        }

        public override Stream Body => _body;
        public override HttpHeadersCollection Headers { get; }
        public override IReadOnlyCollection<IHttpCookie> Cookies { get; }
        public override Uri Url { get; }
        public override IEnumerable<ClaimsIdentity> Identities { get; }
        public override string Method { get; }

        public override HttpResponseData CreateResponse()
            => new FakeHttpResponseData(FunctionContext);
    }

    /// <summary>
    /// Fake implementation of <see cref="HttpResponseData"/>.
    /// </summary>
    internal sealed class FakeHttpResponseData(FunctionContext functionContext)
        : HttpResponseData(functionContext)
    {
        public override HttpStatusCode StatusCode { get; set; }
        public override HttpHeadersCollection Headers { get; set; } = [];
        public override Stream Body { get; set; } = new MemoryStream();
        public override HttpCookies Cookies { get; } = new FakeHttpCookies();
    }

    /// <summary>
    /// Fake implementation of <see cref="HttpCookies"/>.
    /// </summary>
    internal sealed class FakeHttpCookies : HttpCookies
    {
        private readonly List<IHttpCookie> _cookies = [];

        public override void Append(string name, string value)
            => _cookies.Add(new FakeHttpCookie(name, value));

        public override void Append(IHttpCookie cookie)
            => _cookies.Add(cookie);

        public override IHttpCookie CreateNew()
            => new FakeHttpCookie(string.Empty, string.Empty);
    }

    /// <summary>
    /// Fake implementation of <see cref="IHttpCookie"/>.
    /// </summary>
    internal sealed class FakeHttpCookie(string name, string value) : IHttpCookie
    {
        public string Name { get; } = name;
        public string Value { get; } = value;
        public DateTimeOffset? Expires { get; } = null;
        public bool? HttpOnly { get; } = null;
        public double? MaxAge { get; } = null;
        public string? Domain { get; } = null;
        public string? Path { get; } = null;
        public SameSite SameSite { get; } = SameSite.None;
        public bool? Secure { get; } = null;
    }
}
