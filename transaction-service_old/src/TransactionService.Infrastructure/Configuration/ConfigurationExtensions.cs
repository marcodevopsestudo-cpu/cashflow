using Microsoft.Extensions.Configuration;

namespace TransactionService.Infrastructure.Configuration;

/// <summary>
/// Provides extension methods to simplify retrieval of configuration values,
/// supporting fallback across multiple keys.
/// </summary>
internal static class ConfigurationExtensions
{
    /// <summary>
    /// Retrieves the first non-empty configuration value found for the provided keys.
    /// </summary>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="keys">A list of configuration keys to evaluate in order.</param>
    /// <returns>
    /// The first non-null and non-whitespace value found; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configuration"/> or <paramref name="keys"/> is null.
    /// </exception>
    public static string? GetSetting(this IConfiguration configuration, params string[] keys)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(keys);

        foreach (var key in keys)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;

            var value = configuration[key];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    /// <summary>
    /// Retrieves a required configuration value from the provided keys.
    /// </summary>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="errorMessage">The error message to include in the exception if no value is found.</param>
    /// <param name="keys">A list of configuration keys to evaluate in order.</param>
    /// <returns>
    /// The first non-null and non-whitespace value found.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configuration"/>, <paramref name="keys"/>, or <paramref name="errorMessage"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no valid configuration value is found for the provided keys.
    /// </exception>
    public static string GetRequiredSetting(this IConfiguration configuration, string errorMessage, params string[] keys)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(errorMessage);

        return configuration.GetSetting(keys)
            ?? throw new InvalidOperationException(errorMessage);
    }
}
