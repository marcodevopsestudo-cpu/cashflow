using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Common.Exceptions;
using TransactionService.Application.Resources;

namespace TransactionService.Application.Common.Pipeline;

/// <summary>
/// Executes validation before the request handler.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">The validators.</param>
    /// <param name="logger">The logger.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    /// <summary>
    /// Validates the request and invokes the next step.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="next">The next delegate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = new List<string>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            failures.AddRange(result.Errors.Where(e => e is not null).Select(e => e.ErrorMessage));
        }

        if (failures.Count == 0)
        {
            return await next();
        }

        var message = string.Join(" | ", failures.Distinct());
        _logger.LogWarning(MessageCatalog.Logs.ValidationFailure, typeof(TRequest).Name, message);
        throw new ValidationApplicationException($"{MessageCatalog.InvalidPayload} {message}");
    }
}
