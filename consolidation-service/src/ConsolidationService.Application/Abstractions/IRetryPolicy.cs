namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Executes an operation using bounded retry and exponential backoff.
/// </summary>
public interface IRetryPolicy
{
    Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
