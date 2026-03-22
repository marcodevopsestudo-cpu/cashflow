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
            .NotEmpty();

        RuleFor(x => x.Message.CorrelationId)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.Message.TransactionIds)
            .NotEmpty();

        RuleForEach(x => x.Message.TransactionIds)
            .GreaterThan(0);

        RuleFor(x => x.MessageId)
            .NotEmpty();
    }
}
