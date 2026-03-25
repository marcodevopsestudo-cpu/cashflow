using ConsolidationService.Application.Messages.Validation;
using FluentValidation;

namespace ConsolidationService.Application.Commands.ProcessConsolidationBatch;

/// <summary>
/// Validates the integrity and required fields of a <see cref="ProcessConsolidationBatchCommand"/>
/// before it is processed by the workflow.
/// </summary>
public sealed class ProcessConsolidationBatchCommandValidator : AbstractValidator<ProcessConsolidationBatchCommand>
{
    /// <summary>
    /// Initializes validation rules for <see cref="ProcessConsolidationBatchCommand"/>.
    /// </summary>
    /// <remarks>
    /// Ensures that all required identifiers and transaction data are present and valid,
    /// preventing invalid messages from reaching the processing pipeline.
    /// </remarks>
    public ProcessConsolidationBatchCommandValidator()
    {
        RuleFor(x => x.Message.BatchId)
            .NotEmpty()
            .WithMessage(ValidationMessages.Command.BatchIdRequired);

        //RuleFor(x => x.Message.CorrelationId)
        //    .NotEmpty()
        //    .WithMessage(ValidationMessages.Command.CorrelationIdRequired)
        //    .MaximumLength(64)
        //    .WithMessage(ValidationMessages.Command.CorrelationIdTooLong);

        RuleFor(x => x.Message.TransactionIds)
            .NotEmpty()
            .WithMessage(ValidationMessages.Command.TransactionIdsRequired);

        RuleForEach(x => x.Message.TransactionIds)
            .NotEmpty()
            .WithMessage(ValidationMessages.Command.TransactionIdRequired);

        //RuleFor(x => x.MessageId)
        //    .NotEmpty()
        //    .WithMessage(ValidationMessages.Command.MessageIdRequired);
    }
}
