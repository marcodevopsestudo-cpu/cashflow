using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Messages.Logs;
using ConsolidationService.Application.Messages.Validation;
using ConsolidationService.Application.Models;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Steps;

/// <summary>
/// Validates loaded transactions individually and isolates invalid items for manual review.
/// </summary>
public sealed class ValidateTransactionsStep
{
    private readonly ITransactionProcessingErrorRepository _errorRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<ValidateTransactionsStep> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateTransactionsStep"/> class.
    /// </summary>
    /// <param name="errorRepository">Repository used to persist transaction processing errors.</param>
    /// <param name="transactionRepository">Repository used to update transaction processing status.</param>
    /// <param name="logger">Logger instance.</param>
    public ValidateTransactionsStep(
        ITransactionProcessingErrorRepository errorRepository,
        ITransactionRepository transactionRepository,
        ILogger<ValidateTransactionsStep> logger)
    {
        _errorRepository = errorRepository;
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Validates the transactions available in the current execution context and moves invalid items to manual review.
    /// </summary>
    /// <param name="context">The batch execution context containing the transactions to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ExecuteAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        var validTransactions = new List<Transaction>();
        var invalidTransactionIds = new List<Guid>();
        var errors = new List<TransactionProcessingError>();
        var occurredOnUtc = DateTime.UtcNow;

        foreach (var transaction in context.Transactions)
        {
            try
            {
                Validate(transaction);
                validTransactions.Add(transaction);
            }
            catch (Exception exception)
            {
                invalidTransactionIds.Add(transaction.TransactionId);

                errors.Add(new TransactionProcessingError(
                    transaction.TransactionId,
                    context.Message.BatchId,
                    exception.Message,
                    occurredOnUtc));

                _logger.LogWarning(
                    BatchLogMessages.Workflow.TransactionMovedToManualReview,
                    transaction.TransactionId,
                    context.Message.BatchId,
                    exception.Message);
            }
        }

        context.Transactions = validTransactions;

        if (invalidTransactionIds.Count == 0)
        {
            return;
        }

        await _errorRepository.InsertAsync(errors, cancellationToken);

        await _transactionRepository.MarkAsFailedAsync(
            invalidTransactionIds.ToArray(),
            context.Message.BatchId,
            0,
            TransactionStatus.PendingManualReview,
            cancellationToken);
    }

    /// <summary>
    /// Validates a single transaction against business rules.
    /// </summary>
    /// <param name="transaction">The transaction to validate.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transaction contains an invalid amount or balance date.
    /// </exception>
    private static void Validate(Transaction transaction)
    {
        if (transaction.Amount <= 0)
        {
            throw new InvalidOperationException(ValidationMessages.Transaction.AmountMustBeGreaterThanZero);
        }

    }
}
