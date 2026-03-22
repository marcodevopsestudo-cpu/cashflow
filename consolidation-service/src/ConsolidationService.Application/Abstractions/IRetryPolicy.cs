
/// <summary>
/// Defines a policy for executing operations with retry logic using exponential backoff.
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Executes the specified asynchronous operation with retry support.
    /// </summary>
    /// <param name="operation">
    /// The asynchronous operation to be executed. Receives a <see cref="CancellationToken"/> to support cooperative cancellation.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the execution, including any retry attempts.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous execution of the operation.
    /// </returns>
    /// <remarks>
    /// The implementation is expected to apply a bounded retry strategy with exponential backoff
    /// between attempts, stopping either on success or when the retry limit is reached.
    /// </remarks>
    Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
