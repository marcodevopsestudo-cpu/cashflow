using ConsolidationService.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Infrastructure.Resilience;

/// <summary>
/// Implements bounded retries with exponential backoff and jitter.
/// </summary>
public sealed class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private const int MaxRetryCount = 3;
    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(2);
    private readonly ILogger<ExponentialBackoffRetryPolicy> _logger;

    public ExponentialBackoffRetryPolicy(ILogger<ExponentialBackoffRetryPolicy> logger)
    {
        _logger = logger;
    }

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
                    "Retrying consolidation operation. Attempt: {Attempt}, DelaySeconds: {DelaySeconds}",
                    attempt,
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private static TimeSpan CalculateDelay(int attempt)
    {
        var jitterMilliseconds = Random.Shared.Next(100, 750);
        return TimeSpan.FromMilliseconds((BaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1)) + jitterMilliseconds);
    }
}
