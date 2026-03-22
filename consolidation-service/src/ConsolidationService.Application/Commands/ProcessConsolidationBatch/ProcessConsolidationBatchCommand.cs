using ConsolidationService.Application.Contracts;
using MediatR;

namespace ConsolidationService.Application.Commands.ProcessConsolidationBatch;

/// <summary>
/// Requests the processing of one consolidation batch message.
/// </summary>
public sealed record ProcessConsolidationBatchCommand(ConsolidationBatchMessage Message, string MessageId) : IRequest;
