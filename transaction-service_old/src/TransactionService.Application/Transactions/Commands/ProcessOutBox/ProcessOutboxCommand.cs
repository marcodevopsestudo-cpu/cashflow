using MediatR;
using TransactionService.Application.Transactions.Common;

namespace TransactionService.Application.Transactions.Commands.ProcessOutBox;

/// <summary>
/// Represents the create transaction command.
/// </summary>
public sealed record ProcessOutboxCommand(
    int BatchSize,
    string CorrelationId) : IRequest<OutboxProcessorDto>;
