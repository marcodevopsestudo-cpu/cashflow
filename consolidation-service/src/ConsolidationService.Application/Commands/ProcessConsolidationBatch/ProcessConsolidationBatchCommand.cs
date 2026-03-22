using ConsolidationService.Application.Contracts;
using MediatR;

namespace ConsolidationService.Application.Commands.ProcessConsolidationBatch;

/// <summary>
/// Represents a request to process a single consolidation batch message.
/// </summary>
/// <param name="Message">
/// The batch message containing the transactions and metadata required for consolidation.
/// </param>
/// <param name="MessageId">
/// The unique identifier of the message, typically provided by the messaging infrastructure
/// for tracking, idempotency, and correlation purposes.
/// </param>
public sealed record ProcessConsolidationBatchCommand(
    ConsolidationBatchMessage Message,
    string MessageId
) : IRequest;
