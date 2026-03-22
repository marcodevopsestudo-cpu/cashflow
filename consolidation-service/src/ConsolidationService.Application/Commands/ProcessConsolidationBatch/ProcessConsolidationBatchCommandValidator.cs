using FluentValidation;

namespace ConsolidationService.Application.Commands.ProcessConsolidationBatch;

/// <summary>
/// Validates a consolidation batch before it reaches the workflow.
/// </summary>
public sealed class ProcessConsolidationBatchCommandValidator : AbstractValidator<ProcessConsolidationBatchCommand>
{
    public ProcessConsolidationBatchCommandValidator()
    {
        RuleFor(x => x.Message.BatchId).NotEmpty();
        RuleFor(x => x.Message.CorrelationId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Message.TransactionIds).NotEmpty();
        RuleForEach(x => x.Message.TransactionIds).GreaterThan(0);
        RuleFor(x => x.MessageId).NotEmpty();
    }
}
