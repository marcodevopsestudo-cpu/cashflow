namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Represents a transaction boundary for infrastructure persistence operations.
/// </summary>
public interface IUnitOfWork
{
    Task<IAsyncDisposable> BeginAsync(CancellationToken cancellationToken);
}
