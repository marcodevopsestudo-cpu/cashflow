using Azure.Core.Serialization;
using FluentAssertions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using TransactionService.Api.Contracts.Responses;
using TransactionService.Api.Functions;
using TransactionService.Application.Transactions.Common;
using TransactionService.Application.Transactions.Queries.GetTransactionById;
using TransactionService.Domain.Enums;
using Xunit;

namespace TransactionService.UnitTests.Api;

/// <summary>
/// Unit tests for <see cref="GetTransactionByIdFunction"/>.
/// </summary>
public sealed class GetTransactionByIdFunctionTests
{
    /// <summary>
    /// Ensures that the function returns the transaction when it exists.
    /// </summary>
    [Fact]
    public async Task Run_ShouldReturnOkWithTransactionPayload_WhenTransactionExists()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var transactionId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var transaction = new TransactionDto(
            transactionId,
            "ACC-001",
            TransactionKind.Credit,
            150m,
            "BRL",
            now,
            "Salary payment",
            "Received",
            "corr-1",
            now,
            null);

        mediator
            .Setup(x => x.Send(It.IsAny<GetTransactionByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var function = new GetTransactionByIdFunction(mediator.Object);
        var context = new FakeFunctionContext();
        var request = new FakeHttpRequestData(context, HttpMethod.Get.Method);

        // Act
        var response = await function.Run(request, transactionId, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Body.Position = 0;

        var payload = await JsonSerializer.DeserializeAsync<TransactionResponse>(
            response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        payload.Should().NotBeNull();
        payload!.TransactionId.Should().Be(transactionId);
        payload.AccountId.Should().Be("ACC-001");
        payload.Kind.Should().Be(TransactionKind.Credit);
        payload.Amount.Should().Be(150m);
        payload.Currency.Should().Be("BRL");
        payload.Description.Should().Be("Salary payment");
        payload.Status.Should().Be("Received");

        mediator.Verify(
            x => x.Send(
                It.Is<GetTransactionByIdQuery>(q => q.TransactionId == transactionId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Fake implementation of <see cref="FunctionContext"/> for testing purposes.
    /// </summary>
    internal sealed class FakeFunctionContext : FunctionContext
    {
        private IDictionary<object, object> _items = new Dictionary<object, object>();
        private IServiceProvider _instanceServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeFunctionContext"/> class.
        /// </summary>
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

        /// <summary>
        /// Gets the invocation identifier.
        /// </summary>
        public override string InvocationId { get; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Gets the function identifier.
        /// </summary>
        public override string FunctionId { get; } = Guid.NewGuid().ToString("N");

        public override TraceContext TraceContext { get; } = null!;
        public override BindingContext BindingContext { get; } = null!;
        public override RetryContext RetryContext { get; } = null!;

        /// <summary>
        /// Gets or sets the service provider instance.
        /// </summary>
        public override IServiceProvider InstanceServices { get => _instanceServices; set => _instanceServices = value; }

        public override FunctionDefinition FunctionDefinition { get; } = null!;

        /// <summary>
        /// Gets or sets the function context items.
        /// </summary>
        public override IDictionary<object, object> Items { get => _items; set => _items = value; }

        public override IInvocationFeatures Features { get; } = null!;

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        public override CancellationToken CancellationToken => CancellationToken.None;
    }

    /// <summary>
    /// Fake implementation of <see cref="HttpRequestData"/> for testing purposes.
    /// </summary>
    internal sealed class FakeHttpRequestData : HttpRequestData
    {
        private readonly MemoryStream _body = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeHttpRequestData"/> class.
        /// </summary>
        /// <param name="functionContext">The function context.</param>
        /// <param name="method">The HTTP method.</param>
        public FakeHttpRequestData(FunctionContext functionContext, string method)
            : base(functionContext)
        {
            Headers = [];
            Method = method;
            Url = new Uri("https://localhost/api/transactions");
            Cookies = [];
            Identities = [];
        }

        /// <summary>
        /// Gets the request body.
        /// </summary>
        public override Stream Body => _body;

        /// <summary>
        /// Gets the request headers.
        /// </summary>
        public override HttpHeadersCollection Headers { get; }

        /// <summary>
        /// Gets the request cookies.
        /// </summary>
        public override IReadOnlyCollection<IHttpCookie> Cookies { get; }

        /// <summary>
        /// Gets the request URL.
        /// </summary>
        public override Uri Url { get; }

        /// <summary>
        /// Gets the request identities.
        /// </summary>
        public override IEnumerable<ClaimsIdentity> Identities { get; }

        /// <summary>
        /// Gets the HTTP method.
        /// </summary>
        public override string Method { get; }

        /// <summary>
        /// Creates an HTTP response.
        /// </summary>
        /// <returns>The HTTP response data.</returns>
        public override HttpResponseData CreateResponse()
            => new FakeHttpResponseData(FunctionContext);
    }

    /// <summary>
    /// Fake implementation of <see cref="HttpResponseData"/>.
    /// </summary>
    internal sealed class FakeHttpResponseData(FunctionContext functionContext)
        : HttpResponseData(functionContext)
    {
        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public override HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the response headers.
        /// </summary>
        public override HttpHeadersCollection Headers { get; set; } = [];

        /// <summary>
        /// Gets or sets the response body.
        /// </summary>
        public override Stream Body { get; set; } = new MemoryStream();

        /// <summary>
        /// Gets the response cookies.
        /// </summary>
        public override HttpCookies Cookies { get; } = new FakeHttpCookies();
    }

    /// <summary>
    /// Fake implementation of <see cref="HttpCookies"/>.
    /// </summary>
    internal sealed class FakeHttpCookies : HttpCookies
    {
        private readonly List<IHttpCookie> _cookies = [];

        /// <summary>
        /// Appends a cookie.
        /// </summary>
        public override void Append(string name, string value)
            => _cookies.Add(new FakeHttpCookie(name, value));

        /// <summary>
        /// Appends a cookie.
        /// </summary>
        public override void Append(IHttpCookie cookie)
            => _cookies.Add(cookie);

        /// <summary>
        /// Creates a new cookie.
        /// </summary>
        public override IHttpCookie CreateNew()
            => new FakeHttpCookie(string.Empty, string.Empty);
    }

    /// <summary>
    /// Fake implementation of <see cref="IHttpCookie"/>.
    /// </summary>
    internal sealed class FakeHttpCookie(string name, string value) : IHttpCookie
    {
        /// <summary>
        /// Gets the cookie name.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the cookie value.
        /// </summary>
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
