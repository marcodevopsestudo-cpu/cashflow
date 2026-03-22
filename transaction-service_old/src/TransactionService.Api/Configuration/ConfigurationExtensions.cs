using Microsoft.Extensions.Configuration;

namespace TransactionService.Api.Configuration;

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
    /// Retrieves the first non-empty string array found for the provided keys.
    /// </summary>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="keys">A list of configuration section keys to evaluate in order.</param>
    /// <returns>
    /// The first non-empty string array found; otherwise, an empty array.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configuration"/> or <paramref name="keys"/> is null.
    /// </exception>
    public static string[] GetArraySetting(this IConfiguration configuration, params string[] keys)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(keys);

        foreach (var key in keys)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;

            var section = configuration.GetSection(key);

            // Avoid unnecessary binding if section does not exist
            if (!section.Exists())
                continue;

            var values = section.Get<string[]>();
            if (values is { Length: > 0 })
            {
                return values;
            }
        }

        return Array.Empty<string>();
    }
}
