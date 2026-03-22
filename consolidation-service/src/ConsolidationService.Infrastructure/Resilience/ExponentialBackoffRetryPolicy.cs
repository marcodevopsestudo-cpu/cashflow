using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Messages.Logs;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Infrastructure.Resilience;

/// <summary>
/// Executes operations using a bounded retry strategy with exponential backoff and randomized jitter.
/// </summary>
public sealed class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private const int MaxRetryCount = 3;
    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(2);
    private readonly ILogger<ExponentialBackoffRetryPolicy> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExponentialBackoffRetryPolicy"/> class.
    /// </summary>
    /// <param name="logger">
    /// Logger used to record retry attempts and backoff delays.
    /// </param>
    public ExponentialBackoffRetryPolicy(ILogger<ExponentialBackoffRetryPolicy> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executes the specified asynchronous operation using retry logic with exponential backoff.
    /// </summary>
    /// <param name="operation">
    /// The asynchronous operation to execute. Receives a <see cref="CancellationToken"/>
    /// to support cooperative cancellation.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the execution, including any retry delays.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous execution of the operation.
    /// </returns>
    /// <remarks>
    /// The operation is retried up to a bounded number of times when an exception occurs.
    /// Between attempts, the policy waits using exponential backoff with additional jitter
    /// to reduce retry contention.
    /// </remarks>
    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                attempt++;
                await operation(cancellationToken);
                return;
            }
            catch when (attempt <= MaxRetryCount)
            {
                var delay = CalculateDelay(attempt);

                _logger.LogWarning(
                    BatchLogMessages.Resilience.RetryingOperation,
                    attempt,
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Calculates the delay to apply before the next retry attempt.
    /// </summary>
    /// <param name="attempt">
    /// The current retry attempt number.
    /// </param>
    /// <returns>
    /// A <see cref="TimeSpan"/> representing the backoff delay with randomized jitter.
    /// </returns>
    private static TimeSpan CalculateDelay(int attempt)
    {
        var jitterMilliseconds = Random.Shared.Next(100, 750);

        return TimeSpan.FromMilliseconds(
            (BaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1)) + jitterMilliseconds);
    }
}
