using FluentValidation;
using TransactionService.Application.Resources;

namespace TransactionService.Application.Transactions.Commands.CreateTransaction;

/// <summary>
/// Validates the create transaction command.
/// </summary>
public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTransactionCommandValidator"/> class.
    /// </summary>
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().WithMessage(MessageCatalog.AccountIdRequired);
        RuleFor(x => x.Kind).NotEmpty().WithMessage(MessageCatalog.KindRequired);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage(MessageCatalog.AmountMustBeGreaterThanZero);
        RuleFor(x => x.Currency).NotEmpty().WithMessage(MessageCatalog.CurrencyRequired);
        RuleFor(x => x.CorrelationId).NotEmpty().WithMessage(MessageCatalog.CorrelationIdRequired);
        RuleFor(x => x.IdempotencyKey).NotEmpty().WithMessage(MessageCatalog.IdempotencyKeyRequired);
    }
}
