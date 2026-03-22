using FluentValidation;
using TransactionService.Application.Resources;
using TransactionService.Application.Transactions.Commands.ProcessOutBox;

namespace TransactionService.Application.Transactions.Commands.ProcessOutbox;

/// <summary>
/// Validates the create transaction command.
/// </summary>
public sealed class ProcessOutboxCommandValidator : AbstractValidator<ProcessOutboxCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessOutboxCommandValidator"/> class.
    /// </summary>
    public ProcessOutboxCommandValidator()
    {
        RuleFor(x => x.BatchSize).NotEmpty().GreaterThan(0).WithMessage(MessageCatalog.PageSizeMustBeGreaterThanZero);
    }
}
